namespace  DatabaseInterpreter.Core
{
    public class PaginationHelper
    {
        public static long GetPageCount(long total, long pageSize)
        {
            return  total % pageSize == 0 ? total / pageSize : total / pageSize + 1;
        }

        public static (long StartRowNumber,long EndRowNumber) GetStartEndRowNumber(long pageNumber, int pageSize)
        {
            long startRowNumber = (pageNumber - 1) * pageSize + 1;
            long endRowNumber = pageNumber * pageSize;           

            return (startRowNumber, endRowNumber);
        }
    }   
}
