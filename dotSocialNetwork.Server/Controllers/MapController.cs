using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using dotSocialNetwork.Server.Models;
using Npgsql;

namespace dotSocialNetwork.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class MapController : Controller
    {
        //private const string ConnectionString = "Host=fis-hcs.energoatlas.ru;Username=fishcsimportappuser;Password=EnBd6Uhi;Database=fis_hcs";
        //private const string ConnectionString = "Host=192.168.0.165;Username=postgres;Password=postgres;Database=postgres";
        private const string ConnectionString = "Host=45.130.214.139;Username=postgres;Password=dfcz333;Database=postgres";
        [HttpGet("gethouses")]
        public IActionResult GetHouses(int start = 0, int count = 100)
        {
            // Список для хранения результатов
            var houses = new List<HouseData>();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                // Открываем соединение
                connection.Open();

                // SQL-запрос
                var query = @"
                    SELECT 
                        ih.ihs_ee_class, 
                        hp.hpl_geo_latitude, 
                        hp.hpl_geo_longitude
                    FROM 
                        imported_house ih
                    JOIN 
                        house_placement hp 
                    ON 
                        ih.ihs_house_guid = hp.ihs_house_guid
                    WHERE 
                        ih.ihs_ee_class IS NOT NULL 
                        AND ih.ihs_ee_class <> 'Нет'
                        AND ih.ihs_ee_class <> '-'
                        AND hp.hpl_geo_latitude IS NOT NULL
                        AND hp.hpl_geo_longitude IS NOT NULL
                    LIMIT 30000;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    // Добавляем параметры в команду
                    command.Parameters.AddWithValue("@Offset", start);
                    command.Parameters.AddWithValue("@Limit", count);

                    // Выполняем запрос и читаем данные
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Добавляем данные в список
                            houses.Add(new HouseData
                            {
                                IhsEeClass = reader.GetString(0),
                                HplGeoLatitude = reader.GetDouble(1),
                                HplGeoLongitude = reader.GetDouble(2)
                            });
                        }
                    }
                }
            }

            // Возвращаем результат в формате JSON
            return Ok(houses);
        }
        [HttpGet("housescount")]
        public IActionResult HousesCount()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                // Открываем соединение
                connection.Open();

                // SQL-запрос
                var query = @"
                SELECT 
                    COUNT(*) AS house_count
                FROM 
                    imported_house ih
                JOIN 
                    house_placement hp 
                ON 
                    ih.ihs_house_guid = hp.ihs_house_guid
                WHERE 
                    ih.ihs_ee_class IS NOT NULL 
                    AND ih.ihs_ee_class <> 'Нет'
                    AND ih.ihs_ee_class <> '-'
                    AND hp.hpl_geo_latitude IS NOT NULL
                    AND hp.hpl_geo_longitude IS NOT NULL
                ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    // Выполняем запрос и читаем данные
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read()) // Перемещаем курсор на первую строку результата
                        {
                            // Получаем значение из первой колонки (COUNT(*))
                            var houseCount = reader.GetInt64(0); // Используем GetInt64, так как COUNT возвращает число
                            return Ok(houseCount); // Возвращаем результат
                        }
                        else
                        {
                            // Если результат пустой, возвращаем 0 или ошибку
                            return Ok(0);
                        }
                    }
                }
            }

        }
    }

    // Модель для хранения данных
    
}

