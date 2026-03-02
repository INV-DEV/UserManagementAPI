using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Model;

namespace UserManagementAPI.DTOs
{
    public class GetUserByIdResponse : ActionResult
    {
        string Message { get; set; }
        User user { get; set; }
    }
}
