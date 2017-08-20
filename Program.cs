using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace USRaceList
{
    class Program
    {
        static DBSql dbSql = new DBSql();
        static USRaceList.ProgramService.ProgramClient prgClient = new USRaceList.ProgramService.ProgramClient("Program-BASIC-S");
        static USRaceList.WageringService.WageringClient wageringClient = new USRaceList.WageringService.WageringClient("Wagering-BASIC-S");
        static USRaceList.PoolInfoService.PoolInfoClient poolClient = new USRaceList.PoolInfoService.PoolInfoClient("PoolInfo-BASIC-S");
        static void Main(string[] args)
        {
            
            Console.Title = "US Race List";
            Console.WriteLine("US Race List program started on " + DateTime.Now.ToLongDateString());
            Console.WriteLine();

            //GetEventList();
           //TestPoolInfoService();
            TestWageringService();

            //Console.WriteLine();
            //Console.WriteLine("Press any key  to continue.................");
            //Console.ReadKey();
        }

        private static void TestPoolInfoService()
        {

            USRaceList.PoolInfoService.MatrixRequest req = new PoolInfoService.MatrixRequest();
            USRaceList.PoolInfoService.MatrixResponse res = new PoolInfoService.MatrixResponse();

            wageringClient.ClientCredentials.UseIdentityConfiguration = true;
            poolClient.ClientCredentials.UserName.UserName = "smg-tl";
            poolClient.ClientCredentials.UserName.Password = "SMGsmg2012";
 

            req.Source = GetPoolInfoServiceSource();
            
            req.EventId = "IVM";
            req.PoolId = "WIN";
            req.RaceId = 1;
            req.MatrixFormat = USRaceList.PoolInfoService.MatrixFormat.Odds;
            res = poolClient.GetMatrix(req);

        }


        private static void TestWageringService()
        {
            USRaceList.WageringService.TicketRequest req = new USRaceList.WageringService.TicketRequest();
            USRaceList.WageringService.TicketResponse res = new USRaceList.WageringService.TicketResponse();

            wageringClient.ClientCredentials.UserName.UserName = "smg-tl";
            wageringClient.ClientCredentials.UserName.Password = "SMGsmg2012";
            //wageringClient.ClientCredentials.UseIdentityConfiguration = true;


            req.Source = GetWageringServiceSource();
            req.EventId = "IVM";
            req.RaceId = 1;
            req.CurrencyId = "USD";
            req.AccountId = "569999901";
            req.Pin = "1234";
            req.ReferenceId = "test";
            

            USRaceList.WageringService.Wagers wagers = new USRaceList.WageringService.Wagers();
            USRaceList.WageringService.Wager wager = new USRaceList.WageringService.Wager();
            wager.PoolId = "WIN";

            USRaceList.WageringService.Legs legs = new USRaceList.WageringService.Legs();
            USRaceList.WageringService.Leg leg = new USRaceList.WageringService.Leg();
            leg.Runners = "2";
            legs.Add(leg);

            wager.Legs = legs;
            wagers.Add(wager);
            req.Wagers = wagers;

            wageringClient.ValidateTicket(ref req);

            res = wageringClient.IssueTicket(req);
        }

        private static void GetEventList()
        {
            try
            {
                USRaceList.ProgramService.EventsRequest req = new USRaceList.ProgramService.EventsRequest();
                USRaceList.ProgramService.EventsResponse res = new USRaceList.ProgramService.EventsResponse();
                req.Source = GetProgramServiceSource();

                prgClient.ClientCredentials.UserName.UserName = "smg-tl";
                prgClient.ClientCredentials.UserName.Password = "SMGsmg2012";
                prgClient.ClientCredentials.UseIdentityConfiguration = true;


                res = prgClient.GetEvents(req);

                for (int i = 0; i < res.Events.Count(); i++)
                {
                    //Console.WriteLine(res.Events[i].EventName + " " + res.Events[i].TrackType.ToString());
                    if (res.Events[i].TrackType.ToString() == "Thoroughbred")
                    {
                        string parlay = "";

                        if (res.Events[i].Parlay)
                            parlay = "1";
                        else
                            parlay = "0";


                        string meetingId = res.Events[i].EventTime.ToString("yyyyMMdd") + res.Events[i].TrackId;

                        CheckExistingData("tblUSEvent", "MeetingId", meetingId);

                        string sqlInsert = "Insert into tblUSEvent (MeetingId, EventId, RunId, TrackId, EventName, EventStatus, EventDateTime, EventInfo, EventType, CurrencyId, TrackName, TrackType, TrackCondition, TurfTrack, Parlay) values ('" + meetingId + "','" + res.Events[i].EventId + "','" + res.Events[i].RunId + "','" + res.Events[i].TrackId + "','" + res.Events[i].EventName + "','" + res.Events[i].EventStatus + "','" + res.Events[i].EventTime.ToString() + "','" + res.Events[i].EventInfo + "','" + res.Events[i].EventType.ToString() + "','" + res.Events[i].CurrencyId + "','" + res.Events[i].TrackName + "','" + res.Events[i].TrackType.ToString() + "','" + res.Events[i].TrackCondition + "','" + res.Events[i].TurfTrack + "'," + parlay + ")";
                        dbSql.ExecuteNonQuery(sqlInsert);

                        Console.WriteLine("MeetingId - " + meetingId + ", EventId - " + res.Events[i].EventId + ", EventName - " + res.Events[i].EventName);

                        GetEventDetail(meetingId, res.Events[i].EventId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void CheckExistingData(string tableName, string fieldName, string fieldValue)
        {
            string sql = "Select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'";
            DataSet ds = dbSql.GetDataSet(sql);

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                string sqlDelete = "Delete from " + tableName + " where " + fieldName + " = '" + fieldValue + "'";
                dbSql.ExecuteNonQuery(sqlDelete);
            }
        }

        private static void GetEventDetail(string meetingId, string eventId)
        {
            try
            {
                USRaceList.ProgramService.EventDetailRequest reqDetail = new USRaceList.ProgramService.EventDetailRequest();
                USRaceList.ProgramService.EventDetailResponse resDetail = new USRaceList.ProgramService.EventDetailResponse();
                reqDetail.Source = GetProgramServiceSource();

                reqDetail.EventId = eventId;
                reqDetail.Races = true;
                reqDetail.Program = true;
                reqDetail.Runners = true;
                // reqDetail.Pools = true;

                resDetail = prgClient.GetEventDetail(reqDetail);
               
                for (int j = 0; j < resDetail.EventDetail.Races.Count(); j++)
                {
                    string current = "0", odds = "0";

                    if (resDetail.EventDetail.Races[j].Current)
                        current = "1";

                    if (resDetail.EventDetail.Races[j].Odds)
                        odds = "1";

                    string tabRaceId = "";

                    if (resDetail.EventDetail.Races[j].RaceId < 10)
                        tabRaceId = meetingId + "0" + resDetail.EventDetail.Races[j].RaceId;
                    else
                        tabRaceId = meetingId + resDetail.EventDetail.Races[j].RaceId;


                    CheckExistingData("tblUSEventDetail", "TabRaceId", tabRaceId);


                    string sqlInsert = "Insert into tblUSEventDetail ([TabRaceId],[MeetingId],[RaceNo],[RaceStatus],[RaceCurrent],[PostTime],[NoOfRunners],[Finish],[Odds],[Live],[PoolList],[Conditions],[RaceType],[Surface],[Distance],[Pursue],[Sex],[Age],[Claim]) values ('" + tabRaceId + "','" + meetingId + "'," + resDetail.EventDetail.Races[j].RaceId + ",'" + resDetail.EventDetail.Races[j].RaceStatus + "'," + current + ",'" + resDetail.EventDetail.Races[j].PostTime.ToString() + "'," + resDetail.EventDetail.Races[j].NumberOfRunners + ",'" + resDetail.EventDetail.Races[j].Finish + "'," + odds + ",'" + resDetail.EventDetail.Races[j].Live + "','" + resDetail.EventDetail.Races[j].PoolList + "','" + resDetail.EventDetail.Races[j].Conditions + "','" + resDetail.EventDetail.Races[j].RaceType + "','" + resDetail.EventDetail.Races[j].Surface + "','" + resDetail.EventDetail.Races[j].Distance + "','" + resDetail.EventDetail.Races[j].Purse + "','" + resDetail.EventDetail.Races[j].Sex + "','" + resDetail.EventDetail.Races[j].Age + "','" + resDetail.EventDetail.Races[j].Claim + "')";
                    dbSql.ExecuteNonQuery(sqlInsert);

                    GetRaceDetail(tabRaceId, reqDetail.EventId, resDetail.EventDetail.Races[j].RaceId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void GetRaceDetail(string tabRaceId, string eventId, int raceNo)
        {
            USRaceList.ProgramService.RaceRequest reqRace = new USRaceList.ProgramService.RaceRequest();
            USRaceList.ProgramService.RaceResponse resRace = new USRaceList.ProgramService.RaceResponse();
            reqRace.Source = GetProgramServiceSource();
            reqRace.EventId = eventId;
            reqRace.RaceId = raceNo;
            reqRace.Runners = true;
            reqRace.Pools = true;

            resRace = prgClient.GetRace(reqRace);

            CheckExistingData("tblUSRaceDetail", "TabRaceId", tabRaceId);
          
            for (int j = 0; j < resRace.Race.Runners.Count; j++)
            {
                string scratch = "0";

                if (resRace.Race.Runners[j].Scratch)
                    scratch = "1";

               
                for (int k = 0; k < resRace.Race.Runners[j].Entries.Count; k++)
                {
                    string runnerNo = resRace.Race.Runners[j].RunnerId + resRace.Race.Runners[j].Entries[k].EntryId;
                    string sqlInsert = "Insert into tblUSRaceDetail ([TabRaceId],[RaceNo],[RunnerNo],[EntryId],[Scratch],[ML]) values ('" + tabRaceId + "'," + raceNo + "," + resRace.Race.Runners[j].RunnerId + ",'" + resRace.Race.Runners[j].Entries[k].EntryId + "'," + scratch + ",'" + resRace.Race.Runners[j].Odds + "')";
                    dbSql.ExecuteNonQuery(sqlInsert);

                }
               
            }
            GetRaceOdds(tabRaceId, eventId, raceNo);
        }

        private static void GetRaceOdds(string tabRaceId, string eventId, int raceNo)
        {
            USRaceList.PoolInfoService.MatrixRequest reqMatrix = new USRaceList.PoolInfoService.MatrixRequest();
            USRaceList.PoolInfoService.MatrixResponse respMatrix = new USRaceList.PoolInfoService.MatrixResponse();
            reqMatrix.Source = GetPoolInfoServiceSource();
            reqMatrix.MatrixFormat = USRaceList.PoolInfoService.MatrixFormat.Odds;
            reqMatrix.EventId = eventId;
            reqMatrix.RaceId = raceNo;
            reqMatrix.PoolId = "WIN";

            respMatrix = poolClient.GetMatrix(reqMatrix);
            USRaceList.PoolInfoService.Row objRow = respMatrix.Rows[0];
            for (int i = 0; i < objRow.Columns.Count ; i++)
            {
                int rNo = objRow.Columns[i].ColumnNumber;
                double price = Convert.ToDouble(objRow.Columns[i].Price);
                string odds = objRow.Columns[i].Formatted;
                string sqlUpdate = "UPDATE tblUSRaceDetail SET Odd='"+odds+"',Price="+price/2+" WHERE TabRaceId='"+tabRaceId+"' AND RunnerNo='"+rNo+"'";
                dbSql.ExecuteNonQuery(sqlUpdate);
            }

        }

        static USRaceList.ProgramService.Source GetProgramServiceSource()
        {
            USRaceList.ProgramService.Source source = new USRaceList.ProgramService.Source();
            source.SourceId = "SONTL2";
            source.SystemId = "BAT";

            return source;
        }

        static USRaceList.WageringService.Source GetWageringServiceSource()
        {
            USRaceList.WageringService.Source source = new USRaceList.WageringService.Source();
            source.SourceId = "SOLTST";
            source.SystemId = "TSG";
            return source;
        }
        static USRaceList.PoolInfoService.Source GetPoolInfoServiceSource()
        {
            USRaceList.PoolInfoService.Source source = new USRaceList.PoolInfoService.Source();
            source.SourceId = "SOLTST";
            source.SystemId = "TSG";
            return source;
        }
    }
}
