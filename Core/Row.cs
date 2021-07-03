namespace Core
{
    /// <summary>
    /// Data row
    /// </summary>
    public class Row
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        public string FormatToString()
        {
            return $"Id: {Id}, First Name: {FirstName}, Last Name: {LastName}, City: {City}, State: {State}, Country: {Country}";
        }
    }
}
