namespace InvoicingCore.Models
{
    public class Address
    {
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        public override string ToString()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Line1))
                parts.Add(Line1);

            if (!string.IsNullOrWhiteSpace(Line2))
                parts.Add(Line2);

            var cityLine = string.Join(", ",
                new[] { City, Province }
                    .Where(v => !string.IsNullOrWhiteSpace(v)));

            if (!string.IsNullOrWhiteSpace(cityLine))
                parts.Add(cityLine);

            var postalLine = string.Join(" ",
                new[] { PostalCode, Country }
                    .Where(v => !string.IsNullOrWhiteSpace(v)));

            if (!string.IsNullOrWhiteSpace(postalLine))
                parts.Add(postalLine);

            return string.Join("\n", parts);
        }

    }
}
