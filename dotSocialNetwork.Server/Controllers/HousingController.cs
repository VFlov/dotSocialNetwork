using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.AspNetCore.Cors;
using dotSocialNetwork.Server.Models;
namespace dotSocialNetwork.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HousingController : Controller
{

    private const string ConnectionString = "Host=fis-hcs.energoatlas.ru;Username=fishcsimportappuser;Password=EnBd6Uhi;Database=fis_hcs";
    [HttpGet("getgishouses")]
    public ActionResult GetHouses(double lat1, double lat2, double lon1, double lon2, int page = 1, int pageSize = 100)
    {
        List<HouseData> houses = new();
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();

            // Calculate the offset based on page number and page size
            int offset = (page - 1) * pageSize;

            var sqlQuery = $@"
            SELECT 
                ih.ihs_ee_class AS EeClass, 
                hp.hpl_geo_longitude AS Longitude, 
                hp.hpl_geo_latitude AS Latitude
            FROM 
                imported_house ih
            JOIN 
                house_placement hp 
            ON 
                ih.ihs_house_guid = hp.ihs_house_guid
            WHERE 
                 hp.hpl_geo_latitude BETWEEN @lat1 AND @lat2
                AND hp.hpl_geo_longitude BETWEEN @lon1 AND @lon2
                AND ih.ihs_ee_class IS NOT NULL
                AND ih.ihs_ee_class <> 'Нет'
                AND ih.ihs_ee_class <> '-'
            LIMIT @pageSize
            OFFSET @offset;";

            using (var command = new NpgsqlCommand(sqlQuery, connection))
            {
                command.Parameters.AddWithValue("@lat1", lat1);
                command.Parameters.AddWithValue("@lat2", lat2);
                command.Parameters.AddWithValue("@lon1", lon1);
                command.Parameters.AddWithValue("@lon2", lon2);
                command.Parameters.AddWithValue("@pageSize", pageSize);
                command.Parameters.AddWithValue("@offset", offset); // Add the offset parameter

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        houses.Add(new HouseData
                        {
                            IhsEeClass = reader.GetString(0)[0].ToString(),
                            HplGeoLongitude = reader.GetDouble(1),
                            HplGeoLatitude = reader.GetDouble(2)
                        });
                    }
                }
            }
        }
        return Ok(houses);
    }

}


