﻿namespace Admin.Dtos
{
    public record UserToken
    (
        string? Token,
        string? Expired,
        string? Message
    );
}
