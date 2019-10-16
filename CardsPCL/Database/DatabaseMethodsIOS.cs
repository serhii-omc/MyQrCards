using System;
using System.Collections.Generic;
using System.IO;
using CardsPCL.Database.Tables;
using CardsPCL.Models;
using SQLite;

namespace CardsPCL.Database
{
    // TODO THIS CLASS IS OPTIMIZED FOR IOS PLATFORM. SO USE using (var db = new SQLiteConnection(dbPath)) IN EVERY METHOD
    public class DatabaseMethodsIOS
    {
        static readonly string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ormdemo.db3");
        //SQLiteConnection db = new SQLiteConnection(dbPath, true);

        #region users card methods
        public void InsertUsersCard(string name,
                                    string surname,
                                    string middlename,
                                    string phone,
                                    string email,
                                    string home_phone,
                                    string personal_site,
                                    string education,
                                    string card_name,
                                    string birthdate,
                                    //string home_address,
                                    string country,
                                    string region,
                                    string city,
                                    string address,
                                    string index,
                                    string notes,
                                    string latitude,
                                    string longitude,
                                    bool from_edit = false)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<UsersCardTable>();
                var users_cards_table = db.Table<UsersCardTable>();

                //clearing users_card table
                foreach (var users_card in users_cards_table)
                {
                    RemoveUsersCard(users_card.Id);
                }

                if (String.IsNullOrEmpty(name))
                    name = null;
                if (String.IsNullOrEmpty(surname))
                    surname = null;
                if (String.IsNullOrEmpty(middlename))
                    middlename = null;
                if (String.IsNullOrEmpty(phone))
                    phone = null;
                if (String.IsNullOrEmpty(email))
                    email = null;
                if (String.IsNullOrEmpty(home_phone))
                    home_phone = null;
                if (String.IsNullOrEmpty(personal_site))
                    personal_site = null;
                else
                {
                    if (!personal_site.ToLower().Contains("https://"))
                        personal_site = "https://" + personal_site;
                }
                if (String.IsNullOrEmpty(education))
                    education = null;
                if (String.IsNullOrEmpty(card_name))
                    card_name = null;
                if (String.IsNullOrEmpty(birthdate))
                    birthdate = null;
                if (birthdate != null)
                    birthdate = birthdate.Replace('.', '-');
                if (String.IsNullOrEmpty(country))
                    country = null;
                if (String.IsNullOrEmpty(region))
                    region = null;
                if (String.IsNullOrEmpty(city))
                    city = null;
                if (String.IsNullOrEmpty(address))
                    address = null;
                if (String.IsNullOrEmpty(index))
                    index = null;
                if (String.IsNullOrEmpty(notes))
                    notes = null;
                if (String.IsNullOrEmpty(latitude))
                    latitude = null;
                if (String.IsNullOrEmpty(longitude))
                    longitude = null;

                try { personal_site = personal_site.ToLower(); } catch { }
                try { latitude = latitude.Replace(',', '.'); } catch { }
                try { longitude = longitude.Replace(',', '.'); } catch { }

                db.Insert(new UsersCardTable
                {
                    name = name,
                    surname = surname,
                    middlename = middlename,
                    phone = phone,
                    email = email,
                    home_phone = home_phone,
                    personal_site = personal_site,
                    education = education,
                    card_name = card_name,
                    birthdate = birthdate,
                    //home_address=home_address,
                    country = country,
                    region = region,
                    city = city,
                    address = address,
                    index = index,
                    notes = notes,
                    latitude = latitude,
                    longitude = longitude,
                    from_edit = from_edit
                });
            }
        }
        public bool Card_from_edit()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<UsersCardTable>();
                var users_card_table = db.Table<UsersCardTable>();
                foreach (var users_card in users_card_table)
                {
                    return users_card.from_edit;
                }
                return false;
            }
        }
        public void RemoveUsersCard(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<UsersCardTable>(id);
                db.Delete(item);
            }
        }
        public void ClearUsersCardTable()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<UsersCardTable>();
                var users_card_table = db.Table<UsersCardTable>();
                foreach (var users_card in users_card_table)
                {
                    db.Delete(users_card);
                }
            }
        }
        public PersonalCardModel GetDataFromUsersCard(int? company_id, int subscription_id, string position_alternative, string corporative_phone_alternative)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<UsersCardTable>();
                var users_card_table = db.Table<UsersCardTable>();
                foreach (var user_card in users_card_table)
                {
                    var location_obj = new LocationModel
                    {
                        Country = user_card.country,
                        PostalCode = user_card.index,
                        Region = user_card.region,
                        Address = user_card.address,
                        City = user_card.city,
                        Notes = user_card.notes,
                        Latitude = user_card.latitude,
                        Longitude = user_card.longitude
                    };
                    var employment_obj = new EmploymentModel
                    {
                        Position = GetPositonInCompany(),
                        Phone = GetCorporativePhoneInCompany(),
                        Email = user_card.email,
                        CompanyID = company_id
                    };
                    if (String.IsNullOrEmpty(employment_obj.Position) && !String.IsNullOrEmpty(employment_obj.Phone))
                        employment_obj = new EmploymentModel
                        {
                            Position = position_alternative,
                            Phone = GetCorporativePhoneInCompany(),
                            Email = user_card.email,
                            CompanyID = company_id
                        };
                    else if (String.IsNullOrEmpty(employment_obj.Position) && String.IsNullOrEmpty(employment_obj.Phone))
                        employment_obj = new EmploymentModel
                        {
                            Position = position_alternative,
                            Phone = corporative_phone_alternative,
                            Email = user_card.email,
                            CompanyID = company_id
                        };
                    else if (!String.IsNullOrEmpty(employment_obj.Position) && String.IsNullOrEmpty(employment_obj.Phone))
                        employment_obj = new EmploymentModel
                        {
                            Position = GetPositonInCompany(),
                            Phone = corporative_phone_alternative,
                            Email = user_card.email,
                            CompanyID = company_id
                        };
                    var person_obj = new PersonModel
                    {
                        FirstName = user_card.name,
                        LastName = user_card.surname,
                        MiddleName = user_card.middlename,
                        MobilePhone = user_card.phone,
                        HomePhone = user_card.home_phone,
                        BirthDate = user_card.birthdate,
                        AddressForm = null,
                        Email = user_card.email,
                        SiteUrl = user_card.personal_site,
                        Education = user_card.education,
                        Location = location_obj,
                        SocialNetworks = GetPersonalNetworkList()
                    };
                    var personal_card_obj = new PersonalCardModel
                    {
                        SubscriptionID = subscription_id,
                        Name = user_card.card_name,
                        Culture = "ru_RU",
                        IsPrimary = false,
                        Person = person_obj,
                        Employment = employment_obj
                    };
                    return personal_card_obj;
                }
                return null;
            }
        }
        public bool card_exists()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<UsersCardTable>();
                var users_card_table = db.Table<UsersCardTable>();
                foreach (var user_card in users_card_table)
                {
                    return true;
                }
                return false;
            }
        }
        public string get_card_name()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<UsersCardTable>();
                var users_card_table = db.Table<UsersCardTable>();
                foreach (var user_card in users_card_table)
                {
                    return user_card.card_name;
                }
                return null;
            }
        }
        #endregion users card methods

        #region company card methods
        public void InsertCompanyCard(string name,
                                      string activity,
                                      string position,
                                      string foundedYear,
                                      string customers,
                                      string companyPhone,
                                      string corporativePhone,
                                      string email,
                                      string fax,
                                      string siteUrl,
                                      //string birthdate,
                                      //string home_address,
                                      string country,
                                      string region,
                                      string city,
                                      string address,
                                      string index,
                                      string notes,
                                      string latitude,
                                      string longitude,
                                     string timestampUTC)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<CompanyCardTable>();
                var company_cards_table = db.Table<CompanyCardTable>();

                //clearing company_card table
                foreach (var company_card in company_cards_table)
                {
                    RemoveCompanyCard(company_card.Id);
                }

                if (String.IsNullOrEmpty(name))
                    name = null;
                if (String.IsNullOrEmpty(activity))
                    activity = null;
                if (String.IsNullOrEmpty(position))
                    position = null;
                if (String.IsNullOrEmpty(foundedYear))
                    foundedYear = null;
                if (String.IsNullOrEmpty(customers))
                    customers = null;
                if (String.IsNullOrEmpty(companyPhone))
                    companyPhone = null;
                if (String.IsNullOrEmpty(corporativePhone))
                    corporativePhone = null;
                if (String.IsNullOrEmpty(email))
                    email = null;
                if (String.IsNullOrEmpty(fax))
                    fax = null;
                if (String.IsNullOrEmpty(siteUrl))
                    siteUrl = null;
                else
                {
                    if (!siteUrl.ToLower().Contains("https://"))
                        siteUrl = "https://" + siteUrl;
                }
                if (String.IsNullOrEmpty(country))
                    country = null;
                if (String.IsNullOrEmpty(region))
                    region = null;
                if (String.IsNullOrEmpty(city))
                    city = null;
                if (String.IsNullOrEmpty(address))
                    address = null;
                if (String.IsNullOrEmpty(index))
                    index = null;
                if (String.IsNullOrEmpty(notes))
                    notes = null;
                if (String.IsNullOrEmpty(latitude))
                    latitude = null;
                if (String.IsNullOrEmpty(longitude))
                    longitude = null;
                if (String.IsNullOrEmpty(timestampUTC))
                    timestampUTC = null;
                try { siteUrl = siteUrl.ToLower(); } catch { }
                try { latitude = latitude.Replace(',', '.'); } catch { }
                try { longitude = longitude.Replace(',', '.'); } catch { }

                db.Insert(new CompanyCardTable
                {
                    name = name,
                    activity = activity,
                    position = position,
                    foundedYear = foundedYear,
                    customers = customers,
                    companyPhone = companyPhone,
                    corporativePhone = corporativePhone,
                    fax = fax,
                    email = email,
                    siteUrl = siteUrl,
                    //birthdate=birthdate,
                    //home_address=home_address,
                    country = country,
                    region = region,
                    city = city,
                    address = address,
                    index = index,
                    notes = notes,
                    latitude = latitude,
                    longitude = longitude,
                    timestampUTC = timestampUTC
                });
            }
        }
        public void RemoveCompanyCard(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<CompanyCardTable>(id);
                db.Delete(item);
            }
        }
        public void ClearCompanyCardTable()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<CompanyCardTable>();
                var company_card_table = db.Table<CompanyCardTable>();
                foreach (var company_card in company_card_table)
                {
                    db.Delete(company_card);
                }
            }
        }
        public CompanyCardModel GetDataFromCompanyCard()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<CompanyCardTable>();
                var company_card_table = db.Table<CompanyCardTable>();
                foreach (var company_card in company_card_table)
                {
                    var location_obj = new LocationModel
                    {
                        Country = company_card.country,
                        PostalCode = company_card.index,
                        Region = company_card.region,
                        Address = company_card.address,
                        City = company_card.city,
                        Notes = company_card.notes,
                        Latitude = company_card.latitude,
                        Longitude = company_card.longitude
                    };
                    CompanyCardModel company_card_obj;
                    if (company_card.foundedYear != "0" && !String.IsNullOrEmpty(company_card.foundedYear))
                        company_card_obj = new CompanyCardModel
                        {
                            Name = company_card.name,
                            Phone = company_card.companyPhone,
                            Fax = company_card.fax,
                            Email = company_card.email,
                            SiteUrl = company_card.siteUrl,
                            FoundedYear = Convert.ToInt32(company_card.foundedYear),
                            Activity = company_card.activity,
                            Customers = company_card.customers,
                            Location = location_obj
                        };
                    else
                        company_card_obj = new CompanyCardModel
                        {
                            Name = company_card.name,
                            Phone = company_card.companyPhone,
                            Fax = company_card.fax,
                            Email = company_card.email,
                            SiteUrl = company_card.siteUrl,
                            FoundedYear = null,
                            Activity = company_card.activity,
                            Customers = company_card.customers,
                            Location = location_obj
                        };
                    return company_card_obj;
                }
                return null;
            }
        }
        public string GetPositonInCompany()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<CompanyCardTable>();
                var company_card_table = db.Table<CompanyCardTable>();
                foreach (var company_card in company_card_table)
                {
                    return company_card.position;
                }
                return null;
            }
        }
        public string GetCorporativePhoneInCompany()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<CompanyCardTable>();
                var company_card_table = db.Table<CompanyCardTable>();
                foreach (var company_card in company_card_table)
                {
                    return company_card.corporativePhone;
                }
                return null;
            }
        }
        #endregion company card methods

        #region validTill and repeatAfter methods
        public void InsertValidTillRepeatAfter(DateTime validTill, DateTime repeatAfter, string email)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfter_table = db.Table<ValidTill_RepeatAfterTable>();

                //clearing table
                foreach (var validTillRepeatAfter in validTillRepeatAfter_table)
                {
                    RemoveValidTillRepeatAfter(validTillRepeatAfter.Id);
                }

                db.Insert(new ValidTill_RepeatAfterTable
                {
                    email = email,
                    validTill = validTill,
                    repeatAfter = repeatAfter
                });
            }
        }
        public void RemoveValidTillRepeatAfter(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<ValidTill_RepeatAfterTable>(id);
                db.Delete(item);
            }
        }
        public void ClearValidTillRepeatAfterTable()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfter_table = db.Table<ValidTill_RepeatAfterTable>();
                foreach (var validTillRepeatAfter in validTillRepeatAfter_table)
                {
                    db.Delete(validTillRepeatAfter);
                }
            }
        }
        public DateTime GetValidTill()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfter_table = db.Table<ValidTill_RepeatAfterTable>();
                foreach (var validTillRepeatAfter in validTillRepeatAfter_table)
                {
                    return validTillRepeatAfter.validTill;
                }
                return DateTime.Now;
            }
        }
        public DateTime GetRepeatAfter()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfter_table = db.Table<ValidTill_RepeatAfterTable>();
                foreach (var validTillRepeatAfter in validTillRepeatAfter_table)
                {
                    return validTillRepeatAfter.repeatAfter;
                }
                return DateTime.Now;
            }
        }
        public string GetEmailFromValidTill_RepeatAfter()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfter_table = db.Table<ValidTill_RepeatAfterTable>();
                foreach (var validTillRepeatAfter in validTillRepeatAfter_table)
                {
                    return validTillRepeatAfter.email;
                }
                return "";
            }
        }
        #endregion validTill and repeatAfter methods

        #region loginAfter methods
        public void InsertLoginAfter(string accessJwt, string accountUrl, int subscriptionId)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LoginAfterTable>();
                var loginAfterTable = db.Table<LoginAfterTable>();

                //clearing userd_card table
                foreach (var loginAfterItem in loginAfterTable)
                {
                    RemoveLoginAfter(loginAfterItem.Id);
                }

                db.Insert(new LoginAfterTable
                {
                    accessJwt = accessJwt,
                    accountUrl = accountUrl,
                    subscriptionId = subscriptionId
                });
            }
        }
        public void RemoveLoginAfter(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<LoginAfterTable>(id);
                db.Delete(item);
            }
        }
        public bool userExists()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LoginAfterTable>();
                var loginAfterTable = db.Table<LoginAfterTable>();
                foreach (var loginAfterItem in loginAfterTable)
                {
                    return true;
                }
                return false;
            }
        }
        public string GetAccessJwt()
        {
            //try
            //{
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LoginAfterTable>();
                var loginAfterTable = db.Table<LoginAfterTable>();
                foreach (var loginAfterItem in loginAfterTable)
                {
                    return loginAfterItem.accessJwt;
                }
                //}
                //catch (Exception ex)
                //{

                //}
                return null;

            }
        }
        public int GetLastSubscription()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LoginAfterTable>();
                var loginAfterTable = db.Table<LoginAfterTable>();
                foreach (var loginAfterItem in loginAfterTable)
                {
                    return loginAfterItem.subscriptionId;
                }
                return 0;
            }
        }

        public void CleanLoginAfterTable()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LoginAfterTable>();
                var loginAfterTable = db.Table<LoginAfterTable>();
                foreach (var item in loginAfterTable)
                {
                    RemoveLoginAfter(item.Id);
                }
            }
        }
        #endregion loginAfter methods

        #region loginedFrom methods
        public void InsertLoginedFrom(string path_from)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LoginedFromTable>();
                var loginFromTable = db.Table<LoginedFromTable>();

                //clearing userd_card table
                foreach (var loginFromItem in loginFromTable)
                {
                    RemoveLoginFrom(loginFromItem.Id);
                }

                db.Insert(new LoginedFromTable
                {
                    logined_from_path = path_from
                });
            }
        }
        public void RemoveLoginFrom(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<LoginedFromTable>(id);
                db.Delete(item);
            }
        }
        public string GetLoginedFrom()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LoginedFromTable>();
                var loginAfterTable = db.Table<LoginedFromTable>();
                foreach (var loginAfterItem in loginAfterTable)
                {
                    return loginAfterItem.logined_from_path;
                }
                return null;
            }
        }

        public void CleanLoginedFromTable()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LoginedFromTable>();
                var loginedFromTable = db.Table<LoginedFromTable>();
                foreach (var item in loginedFromTable)
                {
                    RemoveLoginFrom(item.Id);
                }
            }
        }
        #endregion loginedFrom methods

        #region different purposes methods
        //ATTENTION! IF IN THIS TABLE WOULD BE ANOTHER FIELDS THEY WILL BE REMOVED WHILE INSERT
        public void InsertMapHint(bool show)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<DifferentPurposesTable>();
                var differentPurposesTable = db.Table<DifferentPurposesTable>();

                //clearing userd_card table
                foreach (var differentPurposeItem in differentPurposesTable)
                {
                    RemoveDifferentPurpose(differentPurposeItem.Id);
                }

                db.Insert(new DifferentPurposesTable
                {
                    show_map_hint = show
                });
            }
        }
        public void RemoveDifferentPurpose(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<DifferentPurposesTable>(id);
                db.Delete(item);
            }
        }
        public bool GetShowMapHint()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<DifferentPurposesTable>();
                var differentPurposesTable = db.Table<DifferentPurposesTable>();
                foreach (var differentPurposeItem in differentPurposesTable)
                {
                    return differentPurposeItem.show_map_hint;
                }
                return true;
            }
        }
        public void CleanDifferentPurposesTable()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<DifferentPurposesTable>();
                var differentPurposesTable = db.Table<DifferentPurposesTable>();
                foreach (var item in differentPurposesTable)
                {
                    RemoveDifferentPurpose(item.Id);
                }
            }
        }
        #endregion different purposes methods

        #region personal networks table
        public void InsertPersonalNetwork(SocialNetworkModel socialNetwork)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<PersonalSocialNetworkTable>();
                var personalSocialNetworkTable = db.Table<PersonalSocialNetworkTable>();

                //clearing table
                //foreach (var personalSocialNetwork in personalSocialNetworkTable)
                //{
                //    RemovePersonalNetwork(personalSocialNetwork.Id);
                //}

                db.Insert(new PersonalSocialNetworkTable
                {
                    SocialNetworkID = socialNetwork.SocialNetworkID,
                    ContactUrl = socialNetwork.ContactUrl
                });
            }
        }
        public void RemovePersonalNetwork(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<PersonalSocialNetworkTable>(id);
                db.Delete(item);
            }
        }

        public void CleanPersonalNetworksTable()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<PersonalSocialNetworkTable>();
                var personalSocialNetworkTable = db.Table<PersonalSocialNetworkTable>();
                foreach (var personalSocialNetwork in personalSocialNetworkTable)
                {
                    RemovePersonalNetwork(personalSocialNetwork.Id);
                }
            }
        }


        public List<SocialNetworkModel> GetPersonalNetworkList()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<PersonalSocialNetworkTable>();
                var personalSocialNetworkTable = db.Table<PersonalSocialNetworkTable>();
                var social_list = new List<SocialNetworkModel>();
                foreach (var personalSocialNetworkItem in personalSocialNetworkTable)
                {
                    social_list.Add(new SocialNetworkModel { SocialNetworkID = personalSocialNetworkItem.SocialNetworkID, ContactUrl = personalSocialNetworkItem.ContactUrl });
                }
                //if (social_list.Count == 0)
                //return null

                return social_list;
            }
        }
        #endregion personal networks table 


        #region last cloud sync table
        public void InsertLastCloudSync(DateTime dateTime)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LastCloudSyncTable>();
                var lastSyncTable = db.Table<LastCloudSyncTable>();

                //clearing table
                foreach (var record in lastSyncTable)
                {
                    RemoveCloudSyncRecord(record.Id);
                }

                db.Insert(new LastCloudSyncTable
                {
                    dateTime = dateTime
                });
            }
        }
        public void RemoveCloudSyncRecord(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<LastCloudSyncTable>(id);
                db.Delete(item);
            }
        }

        public void CleanCloudSynTable()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LastCloudSyncTable>();
                var cloudSyncTable = db.Table<LastCloudSyncTable>();
                foreach (var record in cloudSyncTable)
                {
                    RemoveCloudSyncRecord(record.Id);
                }
            }
        }

        public DateTime? GetLastCloudSync()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<LastCloudSyncTable>();
                var cloudSyncTable = db.Table<LastCloudSyncTable>();
                foreach (var record in cloudSyncTable)
                {
                    return record.dateTime;
                }

                return null;
            }
        }
        #endregion last cloud sync table

        #region card names methods
        public void InsertCardName(string card_name, int card_id, string card_url, bool isLogoStandard, string personName, string personSurname)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<CardNames>();
                var cardNamesTable = db.Table<CardNames>();

                //clearing userd_card table
                //foreach (var loginFromItem in loginFromTable)
                //{
                //    RemoveLoginFrom(loginFromItem.Id);
                //}
                try
                {
                    db.Insert(new CardNames
                    {
                        card_name = card_name,
                        card_id = card_id,
                        card_url = card_url,
                        isLogoStandard = isLogoStandard,
                        PersonName = personName,
                        PersonSurname = personSurname
                    });
                }
                catch (Exception ex)
                {

                }
            }
        }
        public void RemoveCardName(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<CardNames>(id);
                db.Delete(item);
            }
        }
        public void CleanCardNames()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (SQLiteConnection db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<CardNames>();
                var cardNamesTable = db.Table<CardNames>();
                foreach (var record in cardNamesTable)
                {
                    RemoveCardName(record.Id);
                }
            }
        }
        public List<string> GetCardNames()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var card_names = new List<string>();
                db.CreateTable<CardNames>();
                var cardNamesTable = db.Table<CardNames>();
                foreach (var card_name in cardNamesTable)
                {
                    card_names.Add(card_name.card_name);
                }
                return card_names;
            }
        }
        public List<CardNames> GetCardNamesWithUrlAndIdAndPersonData()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var card_names = new List<CardNames>();
                db.CreateTable<CardNames>();
                var cardNamesTable = db.Table<CardNames>();
                foreach (var item in cardNamesTable)
                {
                    card_names.Add(new CardNames { card_name = item.card_name, card_id = item.card_id, card_url = item.card_url, isLogoStandard = item.isLogoStandard, PersonName = item.PersonName, PersonSurname = item.PersonSurname });//card_name.card_name);
                }
                return card_names;
            }
        }
        #endregion card names methods


        #region actionJwt methods
        public void InsertActionJwt(string actionJwt)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ActionJWT>();
                var actionJWTTable = db.Table<ActionJWT>();

                //clearing userd_card table
                foreach (var item in actionJWTTable)
                {
                    RemoveActionJwt(item.Id);
                }

                db.Insert(new ActionJWT
                {
                    actionJwt = actionJwt
                });
            }
        }
        public void RemoveActionJwt(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<ActionJWT>(id);
                db.Delete(item);
            }
        }

        public string GetActionJwt()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ActionJWT>();
                var actionJWTTable = db.Table<ActionJWT>();
                foreach (var item in actionJWTTable)
                {
                    return item.actionJwt;
                }
                return null;
            }
        }
        #endregion actionJwt methods

        #region eTag methods
        public void InsertEtag(string etag)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ETag>();
                var etagTable = db.Table<ETag>();

                //clearing userd_card table
                foreach (var item in etagTable)
                {
                    RemoveEtag(item.Id);
                }

                db.Insert(new ETag
                {
                    eTag = etag
                });
            }
        }
        public void RemoveEtag(int id)
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                var item = db.Get<ETag>(id);
                db.Delete(item);
            }
        }
        public string GetEtag()
        {
            //if (db == null)
            //    db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ETag>();
                var etagTable = db.Table<ETag>();
                foreach (var item in etagTable)
                {
                    return item.eTag;
                }
                return null;
            }
        }

        public void CleanEtagTable()
        {
            //if (db == null)
            //db = new SQLiteConnection(dbPath);
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<ETag>();
                var etagTable = db.Table<ETag>();
                foreach (var item in etagTable)
                {
                    RemoveEtag(item.Id);
                }
            }
        }
        #endregion eTag methods
    }
}
