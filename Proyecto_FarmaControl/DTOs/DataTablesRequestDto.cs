namespace Proyecto_FarmaControl.DTOs
{
    public class DataTablesRequestDto
    {
        public int Draw { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public DataTablesSearchDto? Search { get; set; }

        public List<DataTablesOrderDto> Order { get; set; } = [];

        public List<DataTablesColumnDto> Columns { get; set; } = [];
    }

    public class DataTablesSearchDto
    {
        public string? Value { get; set; }

        public bool Regex { get; set; }
    }

    public class DataTablesOrderDto
    {
        public int Column { get; set; }

        public string Dir { get; set; } = "asc";
    }

    public class DataTablesColumnDto
    {
        public string? Data { get; set; }

        public string? Name { get; set; }

        public bool Searchable { get; set; }

        public bool Orderable { get; set; }

        public DataTablesSearchDto? Search { get; set; }
    }
}