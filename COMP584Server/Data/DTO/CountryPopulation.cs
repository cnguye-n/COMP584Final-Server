namespace COMP584Server.Data.DTO
{
    public class CountryPopulation
    {
        public required string Name { get; set; }
        public required int Id { get; set; }
        public double Population { get; set; }
        public required string Iso2 { get; set; }
        public required string Iso3 { get; set; }
    }
}
