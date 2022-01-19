using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PenggunaAPI.Auth;
using PenggunaAPI.Data;
using PenggunaAPI.InputMutation;
using PenggunaAPI.Kafka;
using PenggunaAPI.Models;
using PenggunaAPI.OutputMutation;

namespace PenggunaAPI.GraphQL
{
    public class Mutation
    {
        public async Task<Status> RegisterAsync(
            Register input,
            [Service] PenggunaDbContext db
        )
        {
            var pengguna = db.Penggunas.Where(o => o.Username == input.Username).FirstOrDefault();
            if (pengguna != null)
            {
                return new Status(false, "Usename must be unique");
            }
            var newPengguna = new Pengguna
            {
                Firstname = input.Firstname,
                Lastname = input.Lastname,
                Email = input.Email,
                Username = input.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                Created = DateTime.Now,
                isLocked = false
            };
            db.Penggunas.Add(newPengguna);
            await db.SaveChangesAsync();
            var currentPengguna = db.Penggunas.Where(o => o.Username == input.Username).FirstOrDefault();

            var newSaldo = new Saldo
            {
                PenggunaId = currentPengguna.PenggunaId,
                TotalSaldo = input.Saldo,
                MutasiSaldo = null,
                Created = DateTime.Now
            };
            db.Saldos.Add(newSaldo);
            await db.SaveChangesAsync();

            var role = db.Roles.Where(o => o.Name == "Pengguna").FirstOrDefault();
            if (role == null)
            {
                var newRole = new Role
                {
                    Name = "Pengguna"
                };
                db.Roles.Add(newRole);
                await db.SaveChangesAsync();
            }

            var currentRole = db.Roles.Where(o => o.Name == "Pengguna").FirstOrDefault();
            var newUserRole = new UserRole
            {
                PenggunaId = currentPengguna.PenggunaId,
                RoleId = currentRole.RoleId
            };
            db.UserRoles.Add(newUserRole);
            await db.SaveChangesAsync();
            return new Status(true, "New Pengguna Registration Successful");
        }

        public async Task<TokenPengguna> LoginAsync(
            Login input,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] PenggunaDbContext db
        )
        {
            var pengguna = db.Penggunas.Where(o => o.Username == input.Username).FirstOrDefault();
            if (pengguna == null)
            {
                return await Task.FromResult(new TokenPengguna(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, pengguna.Password);
            if (valid)
            {
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, pengguna.Username));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, pengguna.PenggunaId.ToString()));
                var userRoles = db.UserRoles.Where(o => o.PenggunaId == pengguna.PenggunaId).ToList();

                foreach (var userRole in userRoles)
                {
                    var role = db.Roles.Where(o => o.RoleId == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddMinutes(30);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    claims: claims,
                    expires: expired,
                    signingCredentials: credentials
                );

                return await Task.FromResult(
                    new TokenPengguna(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), "Login Success"));
            }
            return await Task.FromResult(new TokenPengguna(null, null, "Username or password was invalid"));
        }

        [Authorize(Roles = new[] { "Pengguna" })]
        public async Task<Status> OrderAsync(
            OrderInput input,
            [Service] PenggunaDbContext db,
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var penggunaId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentPengguna = db.Penggunas.Where(o => o.PenggunaId == penggunaId).FirstOrDefault();
            var currentSaldo = db.Saldos.Where(o => o.PenggunaId == currentPengguna.PenggunaId).OrderBy(o => o.SaldoId).LastOrDefault();
            var pricePerKm = db.Prices.FirstOrDefault();

            var d1 = currentPengguna.Latitude * (Math.PI / 180.0);
            var num1 = currentPengguna.Longitude * (Math.PI / 180.0);
            var d2 = input.LatTujuan * (Math.PI / 180.0);
            var num2 = input.LongTujuan * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            var distance = 6378.137 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
            float price = (float)distance * 1;

            if (currentSaldo.TotalSaldo >= price)
            {
                var newOrder = new Order
                {
                    DriverId = null,
                    PenggunaId = currentPengguna.PenggunaId,
                    LatPengguna = currentPengguna.Latitude,
                    LongPengguna = currentPengguna.Longitude,
                    LatDriver = null,
                    LongDriver = null,
                    LatTujuan = input.LatTujuan,
                    LongTujuan = input.LongTujuan,
                    Created = DateTime.Now,
                    Price = price,
                    Status = "Pending"
                };
                // db.Orders.Add(newOrder);
                // await db.SaveChangesAsync();
                var key = "New Order - " + DateTime.Now.ToString();
                var val = JObject.FromObject(newOrder).ToString(Formatting.None);
                await KafkaHelper.SendMessage(kafkaSettings.Value, "Order", key, val);

                var oldSaldo = db.Saldos.Where(o => o.PenggunaId == currentPengguna.PenggunaId).OrderBy(o => o.SaldoId).LastOrDefault();
                var newSaldo = new Saldo()
                {
                    PenggunaId = currentPengguna.PenggunaId,
                    TotalSaldo = oldSaldo.TotalSaldo - price,
                    MutasiSaldo = -price,
                    Created = DateTime.Now
                };
                db.Saldos.Add(newSaldo);
                await db.SaveChangesAsync();
                return new Status(true, $"Order Successful, your order fee: {price.ToString()}");
            }
            else
            {
                return new Status(false, "Order Failed, please check your balance");
            }
        }

        [Authorize(Roles = new[] { "Pengguna" })]
        public async Task<Status> TopUpAsync(
            float topUp,
            [Service] PenggunaDbContext db,
            [Service] IHttpContextAccessor httpContextAccessor
        )
        {
            var penggunaId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentPengguna = db.Penggunas.Where(o => o.PenggunaId == penggunaId).FirstOrDefault();
            var oldSaldo = db.Saldos.Where(o => o.PenggunaId == currentPengguna.PenggunaId).OrderBy(o => o.SaldoId).LastOrDefault();
            if (oldSaldo != null)
            {
                var newSaldo = new Saldo()
                {
                    PenggunaId = currentPengguna.PenggunaId,
                    TotalSaldo = oldSaldo.TotalSaldo + topUp,
                    MutasiSaldo = topUp,
                    Created = DateTime.Now
                };
                db.Saldos.Add(newSaldo);
                await db.SaveChangesAsync();
                return new Status(true, $"Top Up Successful, {topUp} has been added to your balance");
            }
            else
            {
                return new Status(false, "Top Up Failed");
            }
        }

        [Authorize(Roles = new[] { "Pengguna" })]
        public async Task<Status> CancelOrderAsync(
            [Service] PenggunaDbContext db,
            [Service] IHttpContextAccessor httpContextAccessor
        )
        {
            var builder = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();


            var Serverconfig = new ConsumerConfig
            {
                BootstrapServers = config["KafkaSettings:Server"],
                GroupId = "Order",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };
            Console.WriteLine("=================Cancelling Your Order=================");
            using (var consumer = new ConsumerBuilder<string, string>(Serverconfig).Build())
            {
                var topics = new string[] { "Order" };
                consumer.Subscribe(topics);
                try
                {
                    var cr = consumer.Consume(cts.Token);
                    Console.WriteLine($"Consumed record with Topic: {cr.Topic} key: {cr.Message.Key} and value: {cr.Message.Value}");

                    if (cr.Topic == "Order")
                    {
                        Order order = JsonConvert.DeserializeObject<Order>(cr.Message.Value);
                        order.Status = "Cancelled";
                        db.Orders.Add(order);
                    }
                    var cancel = await db.SaveChangesAsync();
                    Console.WriteLine("--> Data was saved into database");
                    Console.WriteLine("--> Your order was cancelled");
                    if (cancel > 0)
                    {
                        var penggunaId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                        var currentPengguna = db.Penggunas.Where(o => o.PenggunaId == penggunaId).FirstOrDefault();
                        var oldSaldo = db.Saldos.Where(o => o.PenggunaId == currentPengguna.PenggunaId).OrderBy(o => o.SaldoId).LastOrDefault();
                        if (oldSaldo != null)
                        {
                            var newSaldo = new Saldo()
                            {
                                PenggunaId = currentPengguna.PenggunaId,
                                TotalSaldo = oldSaldo.TotalSaldo - (float)oldSaldo.MutasiSaldo,
                                MutasiSaldo = -oldSaldo.MutasiSaldo,
                                Created = DateTime.Now
                            };
                            db.Saldos.Add(newSaldo);
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ctrl-C was pressed.
                }
                finally
                {
                    consumer.Close();
                }
            }
            return new Status(true, "Order was cancelled");
        }
    }
}