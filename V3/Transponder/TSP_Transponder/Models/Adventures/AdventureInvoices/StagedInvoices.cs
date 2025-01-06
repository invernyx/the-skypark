using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Attributes;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Contractors;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Models.Transactor;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Models.Transactor.Invoice;

namespace TSP_Transponder.Models.Adventures.AdventureInvoices
{
    class StagedInvoices
    {
        public static List<Invoice> CreateInitialInvoice(Adventure Adv)
        {
            Adv.InvoiceQuote = new List<Invoice>();
            Invoice NewInvoice = null;
            lock(Adv.InvoiceQuote)
            {
                #region BOBs
                NewInvoice = new Invoice()
                {
                    Link = Adv.ID,
                    Title = "%bobservice%",
                    Status = STATUS.QUOTE,
                    PayeeType = ACCOUNTTYPE.SERVICE,
                    PayeeAccount = "bobsaeroservice",
                    ClientType = ACCOUNTTYPE.PRIVATE,
                    ClientAccount = "bank_checking",
                };

                #region Loadmaster
                /*
                if(Adv.Actions.Find(x =>
                {
                    Type t = x.GetType();
                    return t == typeof(cargo_pickup) || t == typeof(cargo_pickup_2);
                }) != null)
                {
                    DateTime? last_staff_loadmaster = Loadmaster.GetLastPay();
                    Fee loadmaster_fee = new Fee() { Code = "staff_loadmaster", Amount = 250 };
                    if (last_staff_loadmaster == null)
                    {
                        NewInvoice.Fees.Add(loadmaster_fee);
                    }
                    else
                    {
                        if (DateTime.Now - last_staff_loadmaster > TimeSpan.FromHours(24))
                        {
                            NewInvoice.Fees.Add(loadmaster_fee);
                        }
                    }
                }
                */
                #endregion

#if DEBUG
                /*
                #region Cargo Value
                float InsuredValue = 0;
                float UninsuredValue = 0;
                float CargoValue = 0;
                foreach (action_base Action in Adv.Actions)
                {
                    Type t = Action.GetType();
                    if (t == typeof(cargo_pickup))
                    {
                        cargo_pickup ActionTyped = (cargo_pickup)Action;

                        CargoValue += ActionTyped.Cargo.Value;
                        if (!ActionTyped.Cargo.CanInsure)
                        {
                            UninsuredValue += ActionTyped.Cargo.Value;
                        }
                    }
                }
                InsuredValue = (CargoValue - UninsuredValue);
                #endregion

                #region Cargo Insurance
                if (InsuredValue > 0)
                {
                    NewInvoice.Fees.Add(new Fee()
                    {
                        Code = "cargo_insured",
                        Amount = (float)Math.Round(InsuredValue * 0.03),
                        Params = new Dictionary<string, dynamic>()
                        {
                            { "Value", Math.Round(InsuredValue) }
                        },
                        Required = false,
                    });
                }
                if (UninsuredValue > 0)
                {
                    NewInvoice.Liability.Add(new Fee()
                    {
                        Code = "cargo_uninsured",
                        ExclIf = "cargo_insured",
                        Amount = (float)Math.Round(UninsuredValue),
                    });
                }
                #endregion

                #region Aircraft Insurance
                AircraftInstance LoadedAcf = Connectors.SimConnection.Aircraft;

                if (Adv.State == Adventure.AState.Saved && LoadedAcf != null && Adv.AircraftCompatible)
                {
                    NewInvoice.Fees.Add(new Fee()
                    {
                        Code = "aircraft_insurance_quote",
                        Amount = 300,
                        Params = new Dictionary<string, dynamic>()
                    {
                        { "Name", LoadedAcf.Manufacturer + " " + LoadedAcf.Model },
                        { "Expire", LoadedAcf.InsuranceExpire != null ? ((DateTime)LoadedAcf.InsuranceExpire).ToString("O") : null },
                    }
                    });

                    NewInvoice.Fees.Add(new Fee()
                    {
                        Code = "aircraft_registration_quote",
                        Amount = 50,
                        Params = new Dictionary<string, dynamic>()
                    {
                        { "Name", LoadedAcf.Manufacturer + " " + LoadedAcf.Model },
                        { "Expire", LoadedAcf.InsuranceExpire != null ? ((DateTime)LoadedAcf.InsuranceExpire).ToString("O") : null },
                    }
                    });
                }
                else
                {
                    NewInvoice.Fees.Add(new Fee()
                    {
                        Code = "aircraft_insurance_quote",
                        Amount = null,
                    });

                    NewInvoice.Fees.Add(new Fee()
                    {
                        Code = "aircraft_registration_quote",
                        Amount = null
                    });
                }
                #endregion

                #region Fuel
                NewInvoice.Fees.Add(new Fee()
                {
                    Code = "fuel_quote",
                    Amount = null
                });
                #endregion
                */
#endif
                if (NewInvoice.Fees.Count > 0 || NewInvoice.Liability.Count > 0)
                {
                    Adv.InvoiceQuote.Add(NewInvoice);
                }
                #endregion

                #region Oceanic Air
                NewInvoice = new Invoice()
                {
                    Link = Adv.ID,
                    Title = "%userrelocation%",
                    Status = Invoice.STATUS.QUOTE,
                    PayeeType = Invoice.ACCOUNTTYPE.SERVICE,
                    PayeeAccount = "oceanicair",
                    ClientType = Invoice.ACCOUNTTYPE.PRIVATE,
                    ClientAccount = "bank_checking",
                };

                #region Future Relocation Fees
                BsonDocument LastLocationEntry = null;
                GeoLoc LastLoc = null;
                double Distance = 0;
                lock (LiteDbService.DB)
                {
                    var DBCollection = LiteDbService.DB.Database.GetCollection("location_history");
                    if (DBCollection.Count() > 0)
                    {
                        LastLocationEntry = DBCollection.FindAll().Last();
                        if (LastLocationEntry != null)
                        {
                            LastLoc = new GeoLoc((double)LastLocationEntry["Longitude"], (double)LastLocationEntry["Latitude"]);
                            Distance = Math.Round(Utils.MapCalcDist(Adv.Situations[0].Location, LastLoc));
                        }
                    }
                }
                if (Distance != 0)
                {
                    if (LastLocationEntry != null)
                    {
                        Countries.Country from = Countries.GetCountry(LastLoc);
                        Countries.Country to = Countries.GetCountry(Adv.Situations[0].Location);

                        Airport toAirport = Adv.Situations[0].Airport;
                        string From = (string)LastLocationEntry["Name"];
                        string To = toAirport != null ? toAirport.ICAO : Adv.Template.SituationLabels[0] != string.Empty ? Adv.Template.SituationLabels[0] : Adv.Situations[0].Location.ToString(3);
                        NewInvoice.Description = "From " + From + " to " + To;

                        #region Define the type of transport
                        string TravelType = "bicycle";
                        if (Distance > 4)
                        {
                            if (Distance > 50)
                            {
                                if (Distance > 150)
                                {
                                    TravelType = "flight";
                                }
                                else
                                {
                                    TravelType = "shuttle";
                                }
                            }
                            else
                            {
                                TravelType = "bus";
                            }
                        }
                        #endregion
                        
                        #region Relocation
                        Fee relocationFee = new Fee()
                        {
                            Code = "relocation",
                            Amount = (float)Math.Round(Distance * LocationHistory.RelocationCost),
                            Params = new Dictionary<string, dynamic>()
                            {
                                { "distance", Distance },
                                { "type", TravelType },
                                { "from", From },
                                { "to", To },
                                { "details", new List<Dictionary<string, dynamic>>() }
                            }
                        };
                        NewInvoice.Fees.Add(relocationFee);
                        #endregion

                        #region Holidays
                        int index = 0;
                        foreach(var country in new Countries.Country[] { from, to })
                        {
                            if(index > 0 && from.Code == to.Code)
                            {
                                break;
                            }

                            if (LocationHistory.RelocationMultipliers.ContainsKey(country.Code))
                            {
                                float factor = 1;
                                foreach (var holiday in LocationHistory.RelocationMultipliers[country.Code])
                                {
                                    factor *= holiday.Value;
                                }
                                relocationFee.Amount += (float)Math.Round((float)relocationFee.Amount * factor) - relocationFee.Amount;
                                relocationFee.Params["details"].Add(new Dictionary<string, dynamic>()
                                {
                                    { "type", "holiday" },
                                    { "country", country.Code },
                                    { "note", string.Join(", ", LocationHistory.RelocationMultipliers[country.Code].Select(x => x.Key).Distinct()) }
                                });
                            }
                            index++;
                        }
                        #endregion

                        // Discount
                        if (UserData.Get("tier") != "prospect")
                        {
                            if (Utils.CalculateLevel(Progress.Progress.XP.Balance) < 5)
                            {
                                #region New Pilot Discount
                                float DiscountPct = 90;
                                relocationFee.Discounts = new List<Discount>();
                                relocationFee.Discounts.Add(new Discount()
                                {
                                    Code = "relocation_discount_xp",
                                    Amount = (float)-Math.Round((float)relocationFee.Amount * (DiscountPct / 100)),
                                    Params = new Dictionary<string, dynamic>()
                                    {
                                        { "percentage", DiscountPct },
                                    }
                                });
                                #endregion
                            }
                            else
                            {
                                #region Reliable Pilot Discount
                                relocationFee.Discounts = new List<Discount>();
                                if (Progress.Progress.Reliability.Balance > 90)
                                {
                                    float DiscountPct = 20;
                                    relocationFee.Discounts.Add(new Discount()
                                    {
                                        Code = "relocation_discount_reliability",
                                        Amount = (float)-Math.Round((float)relocationFee.Amount * (DiscountPct / 100)),
                                        Params = new Dictionary<string, dynamic>()
                                        {
                                            { "percentage", DiscountPct },
                                        }
                                    });
                                }
                                #endregion
                            }
                        }
                    }
                    else
                    {
                        NewInvoice.Fees.Add(new Fee()
                        {
                            Code = "relocation",
                            Amount = 0,
                            Params = new Dictionary<string, dynamic>()
                            {
                                { "from", "another planet" },
                                { "to", Adv.Situations[0].Airport != null ? Adv.Situations[0].Airport.ICAO : Adv.Template.SituationLabels[0] != string.Empty ? Adv.Template.SituationLabels[0] : Adv.Situations[0].Location.ToString(3) },
                            }
                        });
                    }
                }
                #endregion

                if (NewInvoice.Fees.Count > 0 || NewInvoice.Liability.Count > 0)
                {
                    Adv.InvoiceQuote.Add(NewInvoice);
                }
                #endregion
                
                #region Refund
                if (Adv.Template.DiscountFees != null && UserData.Get("tier") != "prospect")
                {
                    lock (Adv.Template.DiscountFees)
                    {
                        foreach (var discount in Adv.Template.DiscountFees)
                        {
                            foreach (var invoice in Adv.InvoiceQuote.Where(x => (string)discount["code"] == x.Title))
                            {
                                float dicountableAmount = (float)invoice.Fees.Where(x => x.Amount != null).Select(x => x.Amount + (x.Discounts != null ? x.Discounts.Sum(x1 => x1.Amount) : 0)).Sum();
                                if (dicountableAmount > 0)
                                {
                                    Fee refund = new Fee()
                                    {
                                        Code = (string)discount["code"],
                                        Amount = (float)Math.Floor(-dicountableAmount * (((float)discount["discount"]) / 100)),
                                        RefundMoment = (MOMENT)EnumAttr.GetEnum(typeof(MOMENT), (string)discount["moment"]),
                                        Params = new Dictionary<string, dynamic>()
                                        {
                                            { "percentage", ((float)discount["discount"]) }
                                        }
                                    };

                                    invoice.Refunds.Add(refund);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            return Adv.InvoiceQuote;
        }
        
        public static List<Invoice> CreateActiveInvoice(Adventure Adv)
        {
            Adv.InvoiceQuote = new List<Invoice>();
            Invoice NewInvoice = null;
            lock (Adv.InvoiceQuote)
            {
                var bonuses = Adv.Actions.FindAll(x => typeof(adventure_bonus) == x.GetType()).Cast<adventure_bonus>();

                foreach(var bonus in bonuses)
                {
                    if (bonus.Triggered)
                    {
                        #region On Time Bonus
                        NewInvoice = new Invoice()
                        {
                            Link = Adv.ID,
                            Title = bonus.Label,
                            Status = STATUS.QUOTE,
                            PayeeType = ACCOUNTTYPE.SERVICE,
                            PayeeAccount = bonus.Company,
                            MomentCondition = MOMENT.SUCCEED,
                            ClientType = ACCOUNTTYPE.PRIVATE,
                            ClientAccount = "bank_checking",
                        };

                        #region Fee
                        var buffer = 30f;
                        var time_trigger = (trigger_time)Adv.Actions.Find(x => typeof(trigger_time) == x.GetType());
                        var time_diff = Math.Abs((((DateTime)time_trigger.TriggerTime) - ((DateTime)time_trigger.Triggered)).TotalMinutes);
                        var factor = (time_diff / buffer) * 0.15;

                        if (Math.Abs(time_diff) < buffer)
                        {
                            Fee relocationFee = new Fee()
                            {
                                Code = "bonus_ontime_dep",
                                Amount = (float)Math.Round(-Adv.RewardBux * factor),
                                Params = new Dictionary<string, dynamic>()
                                {
                                    { "percent", (factor * 100) },
                                    { "time", time_trigger.Triggered != null ? ((DateTime)time_trigger.Triggered).ToString("O") : null },
                                }
                            };
                            NewInvoice.Fees.Add(relocationFee);
                        }
                        #endregion
                        
                        if (NewInvoice.Fees.Count > 0 || NewInvoice.Liability.Count > 0)
                        {
                            Adv.InvoiceQuote.Add(NewInvoice);
                        }
                        #endregion
                    }

                }
            }

            return Adv.InvoiceQuote;
        }
        
        public static List<Invoice> CreateResumeInvoice(Adventure Adv)
        {
            Adv.InvoiceQuote = new List<Invoice>();
            Invoice NewInvoice = null;

            // Make the new things
            NewInvoice = new Invoice()
            {
                Link = Adv.ID,
                Status = Invoice.STATUS.QUOTE,
                PayeeType = Invoice.ACCOUNTTYPE.SERVICE,
                PayeeAccount = "oceanicair",
                ClientType = Invoice.ACCOUNTTYPE.PRIVATE,
                ClientAccount = "bank_checking",
            };

            Adv.InvoiceQuote.Add(NewInvoice);

            return Adv.InvoiceQuote;
        }

        public static List<Invoice> CreateFinalInvoice(Adventure Adv)
        {
            Adv.InvoiceQuote = new List<Invoice>();
            Invoice NewInvoice = null;

            NewInvoice = new Invoice()
            {
                Title = "%invoice_final%",
                Status = Invoice.STATUS.QUOTE,
                PayeeType = Invoice.ACCOUNTTYPE.SERVICE,
                PayeeAccount = "bobsaeroservice",
                ClientType = Invoice.ACCOUNTTYPE.PRIVATE,
                ClientAccount = "bank_checking",
            };

            Adv.InvoiceQuote.Add(NewInvoice);

            return Adv.InvoiceQuote;
        }


    }
}
