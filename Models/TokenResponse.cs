namespace ListClientMVC.Models
{
    public class TokenResponse
    {
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string expires_on { get; set; }
        public string resource { get; set; }
        public string access_token { get; set; }
        public string refres_token { get; set; }
    }
}

