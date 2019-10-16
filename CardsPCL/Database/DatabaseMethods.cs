using System;
using System.Collections.Generic;
using System.IO;
using CardsPCL.Database.Tables;
using CardsPCL.Models;
using SQLite;

namespace CardsPCL.Database
{
    public class DatabaseMethods
    {
        static readonly string DbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ormdemo.db3");
        SQLiteConnection _db = new SQLiteConnection(DbPath);

        #region users card methods
        public void InsertUsersCard(string name,
                                    string surname,
                                    string middlename,
                                    string phone,
                                    string email,
                                    string homePhone,
                                    string personalSite,
                                    string education,
                                    string cardName,
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
                                    bool fromEdit = false)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<UsersCardTable>();
            var usersCardsTable = _db.Table<UsersCardTable>();

                //clearing users_card table
            foreach (var usersCard in usersCardsTable)
                {
                    RemoveUsersCard(usersCard.Id);
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
                if (String.IsNullOrEmpty(homePhone))
                    homePhone = null;
                if (String.IsNullOrEmpty(personalSite))
                    personalSite = null;
                else
                {
                    if (!personalSite.ToLower().Contains("https://"))
                        personalSite = "https://" + personalSite;
                }
                if (String.IsNullOrEmpty(education))
                    education = null;
                if (String.IsNullOrEmpty(cardName))
                    cardName = null;
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

                try { personalSite = personalSite.ToLower(); } catch { }
                try { latitude = latitude.Replace(',', '.'); } catch { }
                try { longitude = longitude.Replace(',', '.'); } catch { }

                _db.Insert(new UsersCardTable
                {
                    name = name,
                    surname = surname,
                    middlename = middlename,
                    phone = phone,
                    email = email,
                    home_phone = homePhone,
                    personal_site = personalSite,
                    education = education,
                    card_name = cardName,
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
                    from_edit = fromEdit
                });
            //}
        }
        public bool Card_from_edit()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<UsersCardTable>();
                var usersCardTable = _db.Table<UsersCardTable>();
                foreach (var usersCard in usersCardTable)
                {
                    return usersCard.from_edit;
                }
                return false;
            //}
        }
        public void RemoveUsersCard(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var item = _db.Get<UsersCardTable>(id);
                _db.Delete(item);
            //}
        }
        public void ClearUsersCardTable()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<UsersCardTable>();
                var usersCardTable = _db.Table<UsersCardTable>();
                foreach (var usersCard in usersCardTable)
                {
                    _db.Delete(usersCard);
                }
            //}
        }
        public PersonalCardModel GetDataFromUsersCard(int? companyId, int subscriptionId, string positionAlternative, string corporativePhoneAlternative)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<UsersCardTable>();
                var usersCardTable = _db.Table<UsersCardTable>();
                foreach (var userCard in usersCardTable)
                {
                    var locationObj = new LocationModel
                    {
                        Country = userCard.country,
                        PostalCode = userCard.index,
                        Region = userCard.region,
                        Address = userCard.address,
                        City = userCard.city,
                        Notes = userCard.notes,
                        Latitude = userCard.latitude,
                        Longitude = userCard.longitude
                    };
                    var employmentObj = new EmploymentModel
                    {
                        Position = GetPositonInCompany(),
                        Phone = GetCorporativePhoneInCompany(),
                        Email = userCard.email,
                        CompanyID = companyId
                    };
                    if (String.IsNullOrEmpty(employmentObj.Position) && !String.IsNullOrEmpty(employmentObj.Phone))
                        employmentObj = new EmploymentModel
                        {
                            Position = positionAlternative,
                            Phone = GetCorporativePhoneInCompany(),
                            Email = userCard.email,
                            CompanyID = companyId
                        };
                    else if (String.IsNullOrEmpty(employmentObj.Position) && String.IsNullOrEmpty(employmentObj.Phone))
                        employmentObj = new EmploymentModel
                        {
                            Position = positionAlternative,
                            Phone = corporativePhoneAlternative,
                            Email = userCard.email,
                            CompanyID = companyId
                        };
                    else if (!String.IsNullOrEmpty(employmentObj.Position) && String.IsNullOrEmpty(employmentObj.Phone))
                        employmentObj = new EmploymentModel
                        {
                            Position = GetPositonInCompany(),
                            Phone = corporativePhoneAlternative,
                            Email = userCard.email,
                            CompanyID = companyId
                        };
                    var personObj = new PersonModel
                    {
                        FirstName = userCard.name,
                        LastName = userCard.surname,
                        MiddleName = userCard.middlename,
                        MobilePhone = userCard.phone,
                        HomePhone = userCard.home_phone,
                        BirthDate = userCard.birthdate,
                        AddressForm = null,
                        Email = userCard.email,
                        SiteUrl = userCard.personal_site,
                        Education = userCard.education,
                        Location = locationObj,
                        SocialNetworks = GetPersonalNetworkList()
                    };
                    var personalCardObj = new PersonalCardModel
                    {
                        SubscriptionID = subscriptionId,
                        Name = userCard.card_name,
                        Culture = "ru_RU",
                        IsPrimary = false,
                        Person = personObj,
                        Employment = employmentObj
                    };
                    return personalCardObj;
                }
                return null;
            //}
        }
        public bool card_exists()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<UsersCardTable>();
                var usersCardTable = _db.Table<UsersCardTable>();
                foreach (var userCard in usersCardTable)
                {
                    return true;
                }
                return false;
            //}
        }
        public string get_card_name()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<UsersCardTable>();
                var usersCardTable = _db.Table<UsersCardTable>();
                foreach (var userCard in usersCardTable)
                {
                    return userCard.card_name;
                }
                return null;
            //}
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
                                     string timestampUtc)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<CompanyCardTable>();
                var companyCardsTable = _db.Table<CompanyCardTable>();

                //clearing company_card table
                foreach (var companyCard in companyCardsTable)
                {
                    RemoveCompanyCard(companyCard.Id);
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
                if (String.IsNullOrEmpty(timestampUtc))
                    timestampUtc = null;
                try { siteUrl = siteUrl.ToLower(); } catch { }
                try { latitude = latitude.Replace(',', '.'); } catch { }
                try { longitude = longitude.Replace(',', '.'); } catch { }

                _db.Insert(new CompanyCardTable
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
                    timestampUTC = timestampUtc
                });
            //}
        }
        public void RemoveCompanyCard(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
                var item = _db.Get<CompanyCardTable>(id);
                _db.Delete(item);
            //}
        }
        public void ClearCompanyCardTable()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<CompanyCardTable>();
                var companyCardTable = _db.Table<CompanyCardTable>();
                foreach (var companyCard in companyCardTable)
                {
                    _db.Delete(companyCard);
                }
            //}
        }
        public CompanyCardModel GetDataFromCompanyCard()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<CompanyCardTable>();
                var companyCardTable = _db.Table<CompanyCardTable>();
                foreach (var companyCard in companyCardTable)
                {
                    var locationObj = new LocationModel
                    {
                        Country = companyCard.country,
                        PostalCode = companyCard.index,
                        Region = companyCard.region,
                        Address = companyCard.address,
                        City = companyCard.city,
                        Notes = companyCard.notes,
                        Latitude = companyCard.latitude,
                        Longitude = companyCard.longitude
                    };
                    CompanyCardModel companyCardObj;
                    if (companyCard.foundedYear != "0" && !String.IsNullOrEmpty(companyCard.foundedYear))
                        companyCardObj = new CompanyCardModel
                        {
                            Name = companyCard.name,
                            Phone = companyCard.companyPhone,
                            Fax = companyCard.fax,
                            Email = companyCard.email,
                            SiteUrl = companyCard.siteUrl,
                            FoundedYear = Convert.ToInt32(companyCard.foundedYear),
                            Activity = companyCard.activity,
                            Customers = companyCard.customers,
                            Location = locationObj
                        };
                    else
                        companyCardObj = new CompanyCardModel
                        {
                            Name = companyCard.name,
                            Phone = companyCard.companyPhone,
                            Fax = companyCard.fax,
                            Email = companyCard.email,
                            SiteUrl = companyCard.siteUrl,
                            FoundedYear = null,
                            Activity = companyCard.activity,
                            Customers = companyCard.customers,
                            Location = locationObj
                        };
                    return companyCardObj;
                }
                return null;
            //}
        }
        public string GetPositonInCompany()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<CompanyCardTable>();
                var companyCardTable = _db.Table<CompanyCardTable>();
                foreach (var companyCard in companyCardTable)
                {
                    return companyCard.position;
                }
                return null;
            //}
        }
        public string GetCorporativePhoneInCompany()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<CompanyCardTable>();
                var companyCardTable = _db.Table<CompanyCardTable>();
                foreach (var companyCard in companyCardTable)
                {
                    return companyCard.corporativePhone;
                }
                return null;
            //}
        }
        #endregion company card methods

        #region validTill and repeatAfter methods
        public void InsertValidTillRepeatAfter(DateTime validTill, DateTime repeatAfter, string email)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfterTable = _db.Table<ValidTill_RepeatAfterTable>();

                //clearing table
                foreach (var validTillRepeatAfter in validTillRepeatAfterTable)
                {
                    RemoveValidTillRepeatAfter(validTillRepeatAfter.Id);
                }

                _db.Insert(new ValidTill_RepeatAfterTable
                {
                    email = email,
                    validTill = validTill,
                    repeatAfter = repeatAfter
                });
            //}
        }
        public void RemoveValidTillRepeatAfter(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var item = _db.Get<ValidTill_RepeatAfterTable>(id);
                _db.Delete(item);
            //}
        }
        public void ClearValidTillRepeatAfterTable()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfterTable = _db.Table<ValidTill_RepeatAfterTable>();
                foreach (var validTillRepeatAfter in validTillRepeatAfterTable)
                {
                    _db.Delete(validTillRepeatAfter);
                }
            //}
        }
        public DateTime GetValidTill()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfterTable = _db.Table<ValidTill_RepeatAfterTable>();
                foreach (var validTillRepeatAfter in validTillRepeatAfterTable)
                {
                    return validTillRepeatAfter.validTill;
                }
                return DateTime.Now;
            //}
        }
        public DateTime GetRepeatAfter()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfterTable = _db.Table<ValidTill_RepeatAfterTable>();
                foreach (var validTillRepeatAfter in validTillRepeatAfterTable)
                {
                    return validTillRepeatAfter.repeatAfter;
                }
                return DateTime.Now;
            //}
        }
        public string GetEmailFromValidTill_RepeatAfter()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<ValidTill_RepeatAfterTable>();
                var validTillRepeatAfterTable = _db.Table<ValidTill_RepeatAfterTable>();
                foreach (var validTillRepeatAfter in validTillRepeatAfterTable)
                {
                    return validTillRepeatAfter.email;
                }
                return "";
            //}
        }
        #endregion validTill and repeatAfter methods

        #region loginAfter methods
        public void InsertLoginAfter(string accessJwt, string accountUrl, int subscriptionId)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LoginAfterTable>();
                var loginAfterTable = _db.Table<LoginAfterTable>();

                //clearing userd_card table
                foreach (var loginAfterItem in loginAfterTable)
                {
                    RemoveLoginAfter(loginAfterItem.Id);
                }

                _db.Insert(new LoginAfterTable
                {
                    accessJwt = accessJwt,
                    accountUrl = accountUrl,
                    subscriptionId = subscriptionId
                });
            //}
        }
        public void RemoveLoginAfter(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var item = _db.Get<LoginAfterTable>(id);
                _db.Delete(item);
            //}
        }
        public bool UserExists()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LoginAfterTable>();
                var loginAfterTable = _db.Table<LoginAfterTable>();
                foreach (var loginAfterItem in loginAfterTable)
                {
                    return true;
                }
                return false;
            //}
        }
        public string GetAccessJwt()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LoginAfterTable>();
                var loginAfterTable = _db.Table<LoginAfterTable>();
                foreach (var loginAfterItem in loginAfterTable)
                {
                    return loginAfterItem.accessJwt;
                }
                return null;
            //}
        }
        public int GetLastSubscription()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LoginAfterTable>();
                var loginAfterTable = _db.Table<LoginAfterTable>();
                foreach (var loginAfterItem in loginAfterTable)
                {
                    return loginAfterItem.subscriptionId;
                }
                return 0;
            //}
        }

        public void CleanLoginAfterTable()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LoginAfterTable>();
                var loginAfterTable = _db.Table<LoginAfterTable>();
                foreach (var item in loginAfterTable)
                {
                    RemoveLoginAfter(item.Id);
                }
            //}
        }
        #endregion loginAfter methods

        #region loginedFrom methods
        public void InsertLoginedFrom(string pathFrom)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LoginedFromTable>();
                var loginFromTable = _db.Table<LoginedFromTable>();

                //clearing userd_card table
                foreach (var loginFromItem in loginFromTable)
                {
                    RemoveLoginFrom(loginFromItem.Id);
                }

                _db.Insert(new LoginedFromTable
                {
                    logined_from_path = pathFrom
                });
            //}
        }
        public void RemoveLoginFrom(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var item = _db.Get<LoginedFromTable>(id);
                _db.Delete(item);
            //}
        }
        public string GetLoginedFrom()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LoginedFromTable>();
                var loginAfterTable = _db.Table<LoginedFromTable>();
                foreach (var loginAfterItem in loginAfterTable)
                {
                    return loginAfterItem.logined_from_path;
                }
                return null;
            //}
        }

        public void CleanLoginedFromTable()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LoginedFromTable>();
                var loginedFromTable = _db.Table<LoginedFromTable>();
                foreach (var item in loginedFromTable)
                {
                    RemoveLoginFrom(item.Id);
                }
            //}
        }
        #endregion loginedFrom methods

        #region different purposes methods
        //ATTENTION! IF IN THIS TABLE WOULD BE ANOTHER FIELDS THEY WILL BE REMOVED WHILE INSERT
        public void InsertMapHint(bool show)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<DifferentPurposesTable>();
                var differentPurposesTable = _db.Table<DifferentPurposesTable>();

                //clearing userd_card table
                foreach (var differentPurposeItem in differentPurposesTable)
                {
                    RemoveDifferentPurpose(differentPurposeItem.Id);
                }

                _db.Insert(new DifferentPurposesTable
                {
                    show_map_hint = show
                });
            //}
        }
        public void RemoveDifferentPurpose(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var item = _db.Get<DifferentPurposesTable>(id);
                _db.Delete(item);
            //}
        }
        public bool GetShowMapHint()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<DifferentPurposesTable>();
                var differentPurposesTable = _db.Table<DifferentPurposesTable>();
                foreach (var differentPurposeItem in differentPurposesTable)
                {
                    return differentPurposeItem.show_map_hint;
                }
                return true;
            //}
        }
        public void CleanDifferentPurposesTable()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<DifferentPurposesTable>();
                var differentPurposesTable = _db.Table<DifferentPurposesTable>();
                foreach (var item in differentPurposesTable)
                {
                    RemoveDifferentPurpose(item.Id);
                }
            //}
        }
        #endregion different purposes methods

        #region personal networks table
        public void InsertPersonalNetwork(SocialNetworkModel socialNetwork)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<PersonalSocialNetworkTable>();
                var personalSocialNetworkTable = _db.Table<PersonalSocialNetworkTable>();

                //clearing table
                //foreach (var personalSocialNetwork in personalSocialNetworkTable)
                //{
                //    RemovePersonalNetwork(personalSocialNetwork.Id);
                //}

                _db.Insert(new PersonalSocialNetworkTable
                {
                    SocialNetworkID = socialNetwork.SocialNetworkID,
                    ContactUrl = socialNetwork.ContactUrl
                });
            //}
        }
        public void RemovePersonalNetwork(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var item = _db.Get<PersonalSocialNetworkTable>(id);
                _db.Delete(item);
            //}
        }

        public void CleanPersonalNetworksTable()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<PersonalSocialNetworkTable>();
                var personalSocialNetworkTable = _db.Table<PersonalSocialNetworkTable>();
                foreach (var personalSocialNetwork in personalSocialNetworkTable)
                {
                    RemovePersonalNetwork(personalSocialNetwork.Id);
                }
            //}
        }


        public List<SocialNetworkModel> GetPersonalNetworkList()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<PersonalSocialNetworkTable>();
                var personalSocialNetworkTable = _db.Table<PersonalSocialNetworkTable>();
                var socialList = new List<SocialNetworkModel>();
                foreach (var personalSocialNetworkItem in personalSocialNetworkTable)
                {
                    socialList.Add(new SocialNetworkModel { SocialNetworkID = personalSocialNetworkItem.SocialNetworkID, ContactUrl = personalSocialNetworkItem.ContactUrl });
                }
                //if (social_list.Count == 0)
                //return null

                return socialList;
            //}
        }
        #endregion personal networks table 


        #region last cloud sync table
        public void InsertLastCloudSync(DateTime dateTime)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LastCloudSyncTable>();
                var lastSyncTable = _db.Table<LastCloudSyncTable>();

                //clearing table
                foreach (var record in lastSyncTable)
                {
                    RemoveCloudSyncRecord(record.Id);
                }

                _db.Insert(new LastCloudSyncTable
                {
                    dateTime = dateTime
                });
            //}
        }
        public void RemoveCloudSyncRecord(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var item = _db.Get<LastCloudSyncTable>(id);
                _db.Delete(item);
            //}
        }

        public void CleanCloudSynTable()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LastCloudSyncTable>();
                var cloudSyncTable = _db.Table<LastCloudSyncTable>();
                foreach (var record in cloudSyncTable)
                {
                    RemoveCloudSyncRecord(record.Id);
                }
            //}
        }

        public DateTime? GetLastCloudSync()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<LastCloudSyncTable>();
                var cloudSyncTable = _db.Table<LastCloudSyncTable>();
                foreach (var record in cloudSyncTable)
                {
                    return record.dateTime;
                }

                return null;
            //}
        }
        #endregion last cloud sync table

        #region card names methods
        public void InsertCardName(string cardName, int cardId, string cardUrl, string personName, string personSurname)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<CardNames>();
                var cardNamesTable = _db.Table<CardNames>();

                //clearing userd_card table
                //foreach (var loginFromItem in loginFromTable)
                //{
                //    RemoveLoginFrom(loginFromItem.Id);
                //}
                try
                {
                    _db.Insert(new CardNames
                    {
                        card_name = cardName,
                        card_id = cardId,
                        card_url = cardUrl,
                        PersonName  = personName,
                        PersonSurname = personSurname
                    });
                }catch(Exception ex)
                {

                }
            //}
        }
        public void RemoveCardName(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var item = _db.Get<CardNames>(id);
                _db.Delete(item);
            //}
        }
        public void CleanCardNames()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (SQLiteConnection db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<CardNames>();
                var cardNamesTable = _db.Table<CardNames>();
                foreach (var record in cardNamesTable)
                {
                    RemoveCardName(record.Id);
                }
            //}
        }
        public List<string> GetCardNames()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var cardNames = new List<string>();
                _db.CreateTable<CardNames>();
                var cardNamesTable = _db.Table<CardNames>();
                foreach (var cardName in cardNamesTable)
                {
                    cardNames.Add(cardName.card_name);
                }
                return cardNames;
            //}
        }
        public List<CardNames> GetCardNamesWithUrlAndIdAndPersonData()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var cardNames = new List<CardNames>();
            _db.CreateTable<CardNames>();
            var cardNamesTable = _db.Table<CardNames>();
            foreach (var item in cardNamesTable)
            {
                cardNames.Add(new CardNames { card_name = item.card_name, card_id = item.card_id, card_url = item.card_url, PersonName = item.PersonName, PersonSurname = item.PersonSurname });//card_name.card_name);
            }
            return cardNames;
            //}
        }
        #endregion card names methods


        #region actionJwt methods         public void InsertActionJwt(string actionJwt)         {
             if (_db == null)
                _db = new SQLiteConnection(DbPath);             //using (var db = new SQLiteConnection(dbPath))             //{                 _db.CreateTable<ActionJWT>();                 var actionJwtTable = _db.Table<ActionJWT>();                  //clearing userd_card table                 foreach (var item in actionJwtTable)                 {                     RemoveActionJwt(item.Id);                 }                  _db.Insert(new ActionJWT                 {                     actionJwt = actionJwt                 });             //}         }         public void RemoveActionJwt(int id)         {
             if (_db == null)
                _db = new SQLiteConnection(DbPath);             //using (var db = new SQLiteConnection(dbPath))             //{                 var item = _db.Get<ActionJWT>(id);                 _db.Delete(item);             //}         }          public string GetActionJwt()         {
             if (_db == null)
                _db = new SQLiteConnection(DbPath);             //using (var db = new SQLiteConnection(dbPath))             //{                 _db.CreateTable<ActionJWT>();                 var actionJwtTable = _db.Table<ActionJWT>();                 foreach (var item in actionJwtTable)                 {                     return item.actionJwt;                 }                 return null;             //}         }         #endregion actionJwt methods

        #region eTag methods
        public void InsertEtag(string etag)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<ETag>();
            var etagTable = _db.Table<ETag>();

            //clearing userd_card table
            foreach (var item in etagTable)
            {
                RemoveEtag(item.Id);
            }

            _db.Insert(new ETag
            {
                eTag = etag
            });
            //}
        }
        public void RemoveEtag(int id)
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            var item = _db.Get<ETag>(id);
            _db.Delete(item);
            //}
        }
        public string GetEtag()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<ETag>();
            var etagTable = _db.Table<ETag>();
            foreach (var item in etagTable)
            {
                return item.eTag;
            }
            return null;
            //}
        }

        public void CleanEtagTable()
        {
            if (_db == null)
                _db = new SQLiteConnection(DbPath);
            //using (var db = new SQLiteConnection(dbPath))
            //{
            _db.CreateTable<ETag>();
            var etagTable = _db.Table<ETag>();
            foreach (var item in etagTable)
            {
                RemoveEtag(item.Id);
            }
            //}
        }
        #endregion eTag methods
    }
}
