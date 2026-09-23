namespace eTimeTrial.Web.Backend.Entities
{
    public class ClientPrincipal
    {
        public string IdentityProvider { get; set; }
        public string UserId { get; set; }
        public string UserDetails { get; set; }
        public IEnumerable<ClientPrincipalClaim> Claims { get; set; }
    }

    public class ClientPrincipalClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

}
