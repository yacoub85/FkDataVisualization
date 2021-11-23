namespace FkDataVisualization
{
    class FkAPIData
    {
        private string CountryCode;
        private uint NumberOfMen;
        private uint NumOfWomen;

        public FkAPIData(string countryCode, uint numberOfMen, uint numOfWomen)
        {
            CountryCode = countryCode;
            NumberOfMen = numberOfMen;
            NumOfWomen = numOfWomen;
        }

        public void Set_countryCode(string countryCode) { CountryCode = countryCode; }
        public void Set_number_of_men(uint numberOfMen) { NumberOfMen = numberOfMen; }
        public void Set_number_of_women(uint numOf_Women) { NumOfWomen = numOf_Women; }

        public string Get_country_code() { return CountryCode; }
        public uint Get_number_of_men() { return NumberOfMen; }
        public uint Get_number_of_women() { return NumOfWomen; }
    }
}
