namespace AutomationAPI.MODEL.Interface
{
    public interface IGoogleSheetsService
    {
        Task<IList<string>> GetSpreadsheetsAsync();
        Task<IList<IList<object>>> GetRowsAsync(string spreadsheetId, string range);
    }
}
