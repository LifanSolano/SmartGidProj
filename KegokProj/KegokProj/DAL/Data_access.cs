using KegokProj.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace KegokProj.DAL
{
    public class Data_access
    {
        public Tower AddTower(Towers tower)
        {
            var towerId = new Tower();
            
            using (ZadiagDBEntities context = new ZadiagDBEntities())
            {
                Towers newTower = new Towers();
                newTower.Name = tower.Name;
                newTower.FilePath = tower.FilePath;

                newTower.Lat = tower.Lat;
                newTower.Long = tower.Long;
                
                var checkTower = context.Towers.Any(t => t.Name == newTower.Name && t.Lat == newTower.Lat && t.Long == newTower.Long);

                if (checkTower == false)
                {
                    context.Towers.Add(newTower);
                    context.SaveChanges();
                    towerId.id = newTower.ID;
                }
                else
                {
                    towerId.id = 0;
                }
            }
            return towerId;
        }

        public IQueryable GetTowersList()
        {
            IQueryable<Towers> list;
            ZadiagDBEntities context = new ZadiagDBEntities();

            //var towersList = (from towers in context.Towers
            //                 select new Towers()).ToList<Towers>();
            var towersList = context.Towers.Select(t => new { t.Name });
            
            return towersList;
        }

        public IQueryable GetTowersId()
        {
            IQueryable<Towers> list;
            ZadiagDBEntities context = new ZadiagDBEntities();

            //var towersList = (from towers in context.Towers
            //                 select new Towers()).ToList<Towers>();
            var towersList = context.Towers.Select(t => new { t.ID });

            return towersList;
        }

        public void AddParams(string[] parsedFileContent, int linesCount, Towers tower)
        {
            using (ZadiagDBEntities context = new ZadiagDBEntities())
            {
                FullParams fullParams = new FullParams();

                fullParams.TowerID = tower.ID;
                fullParams.Day = Convert.ToInt32(parsedFileContent[1]);
                fullParams.Month = Convert.ToInt32(parsedFileContent[2]);
                fullParams.Year = Convert.ToInt32(parsedFileContent[3]);
                fullParams.Hour = Convert.ToInt32(parsedFileContent[4]);
                fullParams.Minute = Convert.ToInt32(parsedFileContent[5]);
                fullParams.Second = Convert.ToInt32(parsedFileContent[6]);
                fullParams.PhaseA_CPD = parsedFileContent[7];
                fullParams.PhaseA_BC = parsedFileContent[8];
                fullParams.PhaseB_CPD = parsedFileContent[9];
                fullParams.PhaseB_BC = parsedFileContent[10];
                fullParams.PhaseC_CPD = parsedFileContent[11];
                fullParams.PhaseC_BC = parsedFileContent[12];
                fullParams.Humidity = parsedFileContent[13];
                fullParams.Temp = parsedFileContent[14];
                fullParams.Amplitude_fluct_X = parsedFileContent[15];
                fullParams.Freq_fluct_X = parsedFileContent[16];
                fullParams.Amplitude_fluct_Y = parsedFileContent[17];
                fullParams.Freq_fluct_Y = parsedFileContent[18];
                fullParams.Amplitude_fluct_Z = parsedFileContent[19];
                fullParams.Freq_fluct_Z = parsedFileContent[20];
                fullParams.Angle_XZ = parsedFileContent[21];
                fullParams.Angle_YZ = parsedFileContent[22];

                context.FullParams.Add(fullParams);
                context.SaveChanges();
            }
        }

        public void AddParams(string[] parsedFileContent, int linesCount, string filePath)
        {
            using (ZadiagDBEntities context = new ZadiagDBEntities())
            {
                FullParams fullParams = new FullParams();

                int towerId = context.Towers.Where(t => t.FilePath == filePath).Select(t => t.ID).First();
                fullParams.TowerID = towerId;
                fullParams.Day = Convert.ToInt32(parsedFileContent[1]);
                fullParams.Month = Convert.ToInt32(parsedFileContent[2]);
                fullParams.Year = Convert.ToInt32(parsedFileContent[3]);
                fullParams.Hour = Convert.ToInt32(parsedFileContent[4]);
                fullParams.Minute = Convert.ToInt32(parsedFileContent[5]);
                fullParams.Second = Convert.ToInt32(parsedFileContent[6]);
                fullParams.PhaseA_CPD = parsedFileContent[7];
                fullParams.PhaseA_BC = parsedFileContent[8];
                fullParams.PhaseB_CPD = parsedFileContent[9];
                fullParams.PhaseB_BC = parsedFileContent[10];
                fullParams.PhaseC_CPD = parsedFileContent[11];
                fullParams.PhaseC_BC = parsedFileContent[12];
                fullParams.Humidity = parsedFileContent[13];
                fullParams.Temp = parsedFileContent[14];
                fullParams.Amplitude_fluct_X = parsedFileContent[15];
                fullParams.Freq_fluct_X = parsedFileContent[16];
                fullParams.Amplitude_fluct_Y = parsedFileContent[17];
                fullParams.Freq_fluct_Y = parsedFileContent[18];
                fullParams.Amplitude_fluct_Z = parsedFileContent[19];
                fullParams.Freq_fluct_Z = parsedFileContent[20];
                fullParams.Angle_XZ = parsedFileContent[21];
                fullParams.Angle_YZ = parsedFileContent[22];

                context.FullParams.Add(fullParams);
                context.SaveChanges();
            }
        }

        public int DeleteTower(Towers tower)
        {
            int towerId = 0; //для вытаскивания ID ЛЭПа, чтобы модифицировать data.json
            using (ZadiagDBEntities context = new ZadiagDBEntities())
            {
                if (tower.ID != 0)
                {
                    context.Towers.Attach(tower);
                    context.Towers.Remove(tower);
                    context.SaveChanges();
                    towerId = tower.ID;
                }
                else if (tower.Name != null)
                {
                    Towers towerInDb = context.Towers.First(t => t.Name == tower.Name);
                    towerId = towerInDb.ID;
                    context.Towers.Attach(towerInDb);
                    context.Towers.Remove(towerInDb);
                    context.SaveChanges();
                }
            }
            return towerId;
        }
        
        public int MoveTower(Towers tower)
        {
            int towerId = 0;
            using (ZadiagDBEntities context = new ZadiagDBEntities())
            {
                if (tower.Name != null)
                {
                    var towerInDb = context.Towers.First(t => t.Name == tower.Name);
                    context.Towers.Attach(towerInDb);

                    var entry = context.Entry(towerInDb);

                    towerInDb.Lat = tower.Lat;
                    towerInDb.Long = tower.Long;
                    towerId = towerInDb.ID;
                    entry.State = EntityState.Modified;
                    context.SaveChanges();
                }
                else if (tower.ID != 0)
                {
                    var towerInDb = context.Towers.First(t => t.ID == tower.ID);
                    towerId = towerInDb.ID;
                    context.Towers.Attach(towerInDb);

                    var entry = context.Entry(towerInDb);

                    towerInDb.Lat = tower.Lat;
                    towerInDb.Long = tower.Long;
                    entry.State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
            return towerId;
        }
    }
}