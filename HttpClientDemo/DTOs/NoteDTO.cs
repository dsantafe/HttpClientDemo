namespace HttpClientDemo.DTOs
{
    [XmlRoot("note")]
    public class NoteDTO
    {
        [XmlElement("to")]
        public string? To { get; set; }

        [XmlElement("from")]
        public string? From { get; set; }

        [XmlElement("heading")]
        public string? Heading { get; set; }

        [XmlElement("body")]
        public string? Body { get; set; }
    }
}
