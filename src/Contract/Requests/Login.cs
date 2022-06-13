namespace Contract.Requests;

public static class Login
{
    public class Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}