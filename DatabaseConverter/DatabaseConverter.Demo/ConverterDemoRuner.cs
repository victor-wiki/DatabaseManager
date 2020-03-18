using System.Threading.Tasks;

namespace DatabaseConverter.Demo
{
    public class ConverterDemoRuner
    {
        public static async Task Run(ConverterDemo demo)
        {
            await demo.Convert();
        }
    }
}
