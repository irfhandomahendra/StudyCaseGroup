﻿namespace DriverService.Kafka
{
    public class KafkaSettings
    {
        public string Server { get; set; }
        public int NumPartitions { get; set; }
        public short ReplicationFactor { get; set; }
    }
}