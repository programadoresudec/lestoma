using AutoMapper;

namespace lestoma.DatabaseOffline
{
    public class Mapper
    {
        public static IMapper CreateMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
            });

            return mapperConfiguration.CreateMapper();
        }
    }
}
