using KegokProj.DAL;
using KegokProj.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace KegokProj.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            //using (StreamReader reader = new StreamReader(Server.MapPath("/Content/101_Sensors.txt")))
            //{
            //    //var fileContent = reader.ReadToEnd();
            //    Data_access da = new Data_access();
            //    int linesCount = System.IO.File.ReadAllLines(Server.MapPath("/Content/101_Sensors.txt")).Length;

            //    while (true)
            //    {
            //        var fileByLine = reader.ReadLine();
            //        var parsedFileContent = fileByLine.Split('_');
            //        //da.AddParams(parsedFileContent, linesCount);
            //        if (fileByLine == null) break;
            //    }
            //}
            GetTowersList();
            
            return View();
        }

        public ActionResult Monitoring()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddNewTower(Towers tower)
        {
            List<string> errorsOrSuccess = new List<string>();
            if (ModelState.IsValid)
            {
                TextWriter writer;
                Data_access da = new Data_access(); //записываем в БД
                Tower towerId = da.AddTower(tower);
                
                if (towerId.id != 0)
                {
                    JObject parsed; //распаршенный json
                    using (StreamReader r = new StreamReader(Server.MapPath("/Content/data.json")))
                    {
                        string json = r.ReadToEnd(); //считываем и записываем в строку
                        var items = JsonConvert.DeserializeObject<JObject>(json); //десериализуем строку
                        parsed = JObject.Parse(json);

                        Option option = new Option
                        {
                            iconLayout = "default#image",
                            iconImageHref = "Content/images/electric-tower.png",
                            iconImageSize = new int[] { 30, 42 },
                            iconImageOffset = new int[] { -5, -38 }
                        };

                        Property property = new Property
                        {
                            balloonContentHeader = "<font size=3><b><p id='title'>Редактирование</p></b></font>",
                            balloonContentBody = "<p id='content-balloon'>Широта: <input name='coord1' value='" + tower.Lat + "'></p><p id='content-balloon'> Долгота: <input name='coord2' value='" + tower.Long + "'></p><button id='balloon-btn' iid='" + towerId.id + "' class='btnDelete'>Удалить</button><button iid='" + towerId.id + "' class='btnMove'>Переместить</button></br><button id='footer-btn' class='saveChangesBtn' iid='1'>Сохранить изменения</button>",
                            balloonContentFooter = "",
                            clusterCaption = "",
                            hintContent = "<strong>" + tower.Name + "</strong>"
                        };

                        //List<Geometry> geometry = new List<Geometry>();
                        //geometry.Add(new Geometry()
                        //{
                        //    Type = "Point",
                        //    Coordinates = new double[] { 31.2, 31.222 },
                        //    Options = option,
                        //    Properties = property
                        //});

                        Geometry geometry = new Geometry
                        {
                            type = "Point",
                            coordinates = new double[] { Convert.ToDouble(tower.Lat), Convert.ToDouble(tower.Long) }
                        };
                        var towerToJson = new Tower() { type = "Feature", id = towerId.id, geometry = geometry, options = option, properties = property };

                        ((JArray)parsed.GetValue("features")).Add(
                            new JObject(JObject.FromObject(towerToJson))
                        );

                        
                        List<string> hint = parsed.Descendants().OfType<JProperty>().Where(h => h.Name == "hintContent").Select(h => (string)h.Value).ToList();
                        hint = hint.Select(item => Regex.Replace(item, "<.*?>", String.Empty)).ToList();
                        var towerExists = hint.Contains(tower.Name);
                        
                        //for (int i = 0; i < parsed["features"].Count(); i++)
                        //{
                        //    var qwe = (string)parsed["features"][i]["properties"]["hintContent"];
                        //    var res = Regex.Replace(qwe, "<.*?>", String.Empty);
                        //}
                    }

                    using (writer = new StreamWriter(Server.MapPath("/Content/data.json"), append: false))
                    {
                        writer.WriteLine(JsonConvert.SerializeObject(parsed, Formatting.Indented)); //перезаписываем файл data.json
                    }

                    //using (StreamReader reader = new StreamReader("~/Content/Sensors/" + tower.FilePath))
                    //{
                    //    int linesCount = System.IO.File.ReadAllLines("~/Content/Sensors/" + tower.FilePath).Length;

                    //    while (true)
                    //    {
                    //        var fileByLine = reader.ReadLine();
                    //        var parsedFileContent = fileByLine.Split('_');
                    //        da.AddParams(parsedFileContent, linesCount, tower);
                    //        if (fileByLine == null) break;
                    //    }
                    //}
                    errorsOrSuccess.Add("Опора успешно добавлена!");
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorsOrSuccess.Add("Извините, но на данном месте уже существует опора!");
                }
            }
            return Json(errorsOrSuccess);
        }

        [HttpPost]
        public ActionResult DeleteTower(Towers tower)
        {
            Data_access da = new Data_access();
            int towerId = da.DeleteTower(tower);

            TextWriter writer; //для записи в .json-файл
            JObject parsed; //распаршенный json
            using (StreamReader r = new StreamReader(Server.MapPath("/Content/data.json")))
            {
                string json = r.ReadToEnd(); //считываем и записываем в строку
                var items = JsonConvert.DeserializeObject<JObject>(json); //десериализуем строку
                parsed = JObject.Parse(json);

                for (int i = 0; i < parsed["features"].Count(); i++)
                {
                    var idInJson = (int)parsed["features"][i]["id"];
                    if (idInJson == towerId)
                        parsed["features"][i].Remove();
                }
            }

            using (writer = new StreamWriter(Server.MapPath("/Content/data.json"), append: false))
            {
                writer.WriteLine(JsonConvert.SerializeObject(parsed, Formatting.Indented)); //перезаписываем файл data.json
            }

            return Json("Опора успешно удалена!");
        }

        [HttpPost]
        public JsonResult MoveTower(Towers tower)
        {
            string moveResult = string.Empty;
            if (ModelState.IsValid)
            {
                Data_access da = new Data_access();
                int towerId = da.MoveTower(tower);
                if (towerId != 0)
                {
                    TextWriter writer;
                    JObject parsed; //распаршенный json
                    using (StreamReader r = new StreamReader(Server.MapPath("/Content/data.json")))
                    {
                        string json = r.ReadToEnd(); //считываем и записываем в строку
                        var items = JsonConvert.DeserializeObject<JObject>(json); //десериализуем строку
                        parsed = JObject.Parse(json);

                        for (int i = 0; i < parsed["features"].Count(); i++)
                        {
                            var idInJson = (int)parsed["features"][i]["id"];
                            if (idInJson == towerId)
                            {
                                double[] newCoords = new[] { Convert.ToDouble(tower.Lat), Convert.ToDouble(tower.Long) };
                                string newCoordsInput = "<p id='content-balloon'>Широта: <input name='coord1' value='" + tower.Lat + "'></p><p id='content-balloon'> Долгота: <input name='coord2' value='" + tower.Long + "'></p><button id='balloon-btn' iid='" + towerId + "' class='btnDelete'>Удалить</button><button iid='" + towerId + "' class='btnMove'>Переместить</button></br><button id='footer-btn' class='saveChangesBtn' iid='1'>Сохранить изменения</button>";
                                JToken newCoordsJson = JToken.FromObject(newCoords);
                                JToken newCoordsInputJson = JToken.FromObject(newCoordsInput);
                                parsed["features"][i]["geometry"]["coordinates"].Replace(newCoordsJson);
                                parsed["features"][i]["properties"]["balloonContentBody"].Replace(newCoordsInputJson);
                                break;
                            }
                        }
                    }

                    using (writer = new StreamWriter(Server.MapPath("/Content/data.json"), append: false))
                    {
                        writer.WriteLine(JsonConvert.SerializeObject(parsed, Formatting.Indented)); //перезаписываем файл data.json
                    }
                    moveResult = "Опора перемещена";
                }
                else
                {
                    moveResult = "Не удалось переместить опору";
                }
            }
            return Json(moveResult);
        }
        
        public JsonResult GetTowersList()
        {
            Data_access da = new Data_access();
            IQueryable towersList = da.GetTowersList();
            
            return Json(towersList, JsonRequestBehavior.AllowGet);
        }
    }
}