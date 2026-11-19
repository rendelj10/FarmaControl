namespace Proyecto_FarmaControl.DTOs
{
    public class DataTablesResponseDto<T>
    {
        public int Draw { get; set; }

        public int RecordsTotal { get; set; }

        public int RecordsFiltered { get; set; }

        public IEnumerable<T> Data { get; set; } = [];
    }
}