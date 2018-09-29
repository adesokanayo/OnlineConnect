using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineAD.Api.Domain
{
    
    public class ActiveDirectoryService : IActiveDirectoryService
    {

        #region class members

        #region internal fields

        private string _errorMessage;
        public string LDapConnectionString;
        public string LDapServerLoginUsername;
        public string LDapServerLoginPassword;
        public string UserToLookup;
        public ApplicationUserData UserData;

        #endregion

        #region constructors
        /// <summary>
        /// Default constructor - initialises some members.
        /// </summary>
        public void ActiveDirectoryWrapper()
        {
            _errorMessage = string.Empty;
            UserData = new ApplicationUserData();
            InitUserDetails();

            // LDapConnectionString = ConfigurationManager.AppSettings["ldap_path"];
        } //end default constructors

        #endregion

        #region implementation


        private DirectoryEntry _directoryEntry;

        public ActiveDirectoryService()
        {
        }

        public DirectoryEntry SearchRoot
        {
            get
            {
                return _directoryEntry ?? (_directoryEntry =
                     new DirectoryEntry(LDapConnectionString, "dymcrm", "Controller10$",
                       AuthenticationTypes.Secure));
            }
        }
        /// <summary>
        /// Attempts to get user details from Active directory based on the user's Active Directory login username 
        /// </summary>
        /// <param name="loginUserName">the Active Directory username for the user.</param>
        /// <returns>a DirectoryEntry object containing the user details, null if we could not get the details</returns>
        public SearchResult GetUserDirectoryEntryDetails(string loginUserName)
        {
            try
            {
                var de = new DirectoryEntry(LDapConnectionString,
                  "dymcrm",
                  "Controller10$")
                { AuthenticationType = AuthenticationTypes.Secure };

                var deSearch = new DirectorySearcher
                {
                    SearchRoot = de,
                    Filter = "(&(objectClass=user)(SAMAccountName=" + loginUserName + "))",
                    SearchScope = SearchScope.Subtree
                };
                //SearchResultCollection results;
                //Search the USER object in the hierachy                       
                //Add the attributes which we want to return to the search result          
                var results = deSearch.FindOne();
                return results;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ERROR: " + ex.Message  );
                //ApplicationContext.CurrentHTTPResponse.Write(ex.Message + "<br /><br />");

                while (ex.InnerException != null)
                {
                    _errorMessage += ex.InnerException.Message;
                    //ApplicationContext.CurrentHTTPResponse.Write( ex.InnerException.Message.ToString() + "<br /><br />");
                } //end while
                  //return (errorMessage);
                return null;
            } //end try/catch
        } //end GetUserDirectoryEntryDetails.

        /// <summary>
        /// Populate the encapsualted struct with the data from the Search results
        /// </summary>
        /// <param name="theSearchResult">the SearchResult object which contains the data from Active Directory</param>


        public List<string> Getmemberof(SearchResult theSearchResult)
        {
            var retval = new List<string>();
            if (theSearchResult == null) return retval;
            retval.AddRange(from string obj in theSearchResult.Properties["memberof"] select obj.Split(',') into parser select parser[0].Split('=') into p1 select p1[1] into m1 select m1);

            return retval;
        }
        //public static List<string> Getmembership(SearchResult theSearchResult)
        //{
        //    List<string> Membership = new List<string>();
        //    if (theSearchResult != null)
        //    {
        //        ResultPropertyCollection myResultPropColl;
        //        myResultPropColl = theSearchResult.Properties;
        //        Membership = getmemberof(myResultPropColl);
        //    }
        //}
        public string GetMemberFullname(string userName)
        {
            try
            {
                var dat = new ActiveDirectoryService();
                var details = dat.GetUserDirectoryEntryDetails(userName);
                dat.PopulateUserDataStruct(details);
                var userdata = dat.UserData;
                return userdata.personDisplayname;
            }
            catch
            {
                // ignored
                return string.Empty;
            }
        }

        public ApplicationUserData GetUserData(string userName)
        {
            try
            {
                var dat = new ActiveDirectoryService();
                var details = dat.GetUserDirectoryEntryDetails(userName);

                dat.PopulateUserDataStruct(details);

                return dat.UserData;
            }
            catch
            {
                // ignored
                return new ApplicationUserData();
            }
        }

        public void PopulateUserDataStruct(SearchResult theSearchResult)
        {
            if (theSearchResult != null)
            {
                var myResultPropColl = theSearchResult.Properties;

                /**
                * The code below is usefull for printing out the whole list of attributes from the current user node.
                 * 
                                 
                foreach (string myKey in myResultPropColl.PropertyNames)
                {
                        string tab = "    ";
                        Console.WriteLine(myKey + " = ");
                        foreach (Object myCollection in myResultPropColl[myKey])
                        {
                                Console.WriteLine(tab + myCollection);
                        }//end foreach
                } //end foreach
    */

                //Add first name to search result if it exists
                if (myResultPropColl.Contains("givenName"))
                {
                    UserData.personFirstName = theSearchResult.Properties["givenName"][0].ToString();
                } //end if

                //Add surname to search result if it exists
                if (myResultPropColl.Contains("sn"))
                {
                    UserData.personSurname = theSearchResult.Properties["sn"][0].ToString();
                } //end if

                //Add email to search result if it exists
                if (myResultPropColl.Contains("mail"))
                {
                    UserData.personEmail = theSearchResult.Properties["mail"][0].ToString();
                } //end if

                //Add the job title to search results if it exists
                if (myResultPropColl.Contains("title"))
                {
                    UserData.personJobTitle = theSearchResult.Properties["title"][0].ToString();
                } //end if          

                //Add the person's job description to the search results if it exists
                //if (myResultPropColl.Contains("info"))
                //{
                //    userData.personDepartment = theSearchResult.Properties["info"][0].ToString();

                //} //end if

                //Add the person's county to the search results if it exists
                if (myResultPropColl.Contains("st"))
                {
                    UserData.personCounty = theSearchResult.Properties["st"][0].ToString();

                } //end if

                //Add the person's mobile phone number to the search results if it exists
                if (myResultPropColl.Contains("mobile"))
                {
                    UserData.personMobileNumber = theSearchResult.Properties["mobile"][0].ToString();

                } //end if

                //Add the person's telephone number to the search results if it exists
                if (myResultPropColl.Contains("telephoneNumber"))
                {
                    UserData.personTelephone = theSearchResult.Properties["telephoneNumber"][0].ToString();

                } //end if

                //Add the person's post code to the search results if it exists
                if (myResultPropColl.Contains("postalCode"))
                {
                    UserData.personPostCode = theSearchResult.Properties["postalCode"][0].ToString();

                } //end if

                //Add the person's team (department) to the search results if it exists
                if (myResultPropColl.Contains("department"))
                {
                    UserData.personDepartment = theSearchResult.Properties["department"][0].ToString();

                } //end if

                //Add the person's street address to the search results if it exists
                if (myResultPropColl.Contains("streetAddress"))
                {
                    UserData.personAddress1 = theSearchResult.Properties["streetAddress"][0].ToString();

                } //end if                              

                //Add the person's town to the search results if it exists
                if (myResultPropColl.Contains("l"))
                {
                    UserData.personTown = theSearchResult.Properties["l"][0].ToString();

                } //end if                              

                //Add the person's xx to the search results if it exists
                if (myResultPropColl.Contains("physicalDeliveryOfficeName"))
                {
                    UserData.personLocation = theSearchResult.Properties["physicalDeliveryOfficeName"][0].ToString();

                } //end if 


                if (myResultPropColl.Contains("displayName"))
                {
                    UserData.personDisplayname = theSearchResult.Properties["displayName"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("middlename"))
                {
                    UserData.personMiddlename = theSearchResult.Properties["middlename"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("description"))
                {
                    UserData.personDescription = theSearchResult.Properties["description"][0].ToString();

                } //end if
                if (myResultPropColl.Contains("otherTelephone"))
                {
                    UserData.personOtherTelephone = theSearchResult.Properties["otherTelephone"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("wWWHomePage"))
                {
                    UserData.personWwwHomePage = theSearchResult.Properties["wWWHomePage"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("url"))
                {
                    UserData.personUrl = theSearchResult.Properties["url"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("postOfficeBox"))
                {
                    UserData.personPostOfficeBox = theSearchResult.Properties["postOfficeBox"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("countryCode"))
                {
                    UserData.personCountryCode = theSearchResult.Properties["countryCode"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("co"))
                {
                    UserData.personCountry = theSearchResult.Properties["co"][0].ToString();

                } //end if


                if (myResultPropColl.Contains("userPrincipalName"))
                {
                    UserData.personUserPrincipalName = theSearchResult.Properties["userPrincipalName"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("sAMAccountName"))
                {
                    UserData.personSAMAccountName = theSearchResult.Properties["sAMAccountName"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("homePhone"))
                {
                    UserData.personHomePhone = theSearchResult.Properties["homePhone"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("otherHomePhone"))
                {
                    UserData.personOtherHomePhone = theSearchResult.Properties["otherHomePhone"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("pager"))
                {
                    UserData.personPager = theSearchResult.Properties["pager"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("otherPager"))
                {
                    UserData.personOtherPager = theSearchResult.Properties["otherPager"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("otherMobile"))
                {
                    UserData.personOtherMobile = theSearchResult.Properties["otherMobile"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("facsimileTelephoneNumber"))
                {
                    UserData.personFax = theSearchResult.Properties["facsimileTelephoneNumber"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("otherFacsimileTelephoneNumber"))
                {
                    UserData.personOtherFax = theSearchResult.Properties["otherFacsimileTelephoneNumber"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("ipPhone"))
                {
                    UserData.personIpPhone = theSearchResult.Properties["ipPhone"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("otherIpPhone"))
                {
                    UserData.personOtherIpPhone = theSearchResult.Properties["otherIpPhone"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("company"))
                {
                    UserData.personCompany = theSearchResult.Properties["company"][0].ToString();

                } //end if

                if (myResultPropColl.Contains("manager"))
                {
                    UserData.personManager = theSearchResult.Properties["manager"][0].ToString();

                } //end if


                if (myResultPropColl.Contains("branchCode"))
                {
                    UserData.personLevel = theSearchResult.Properties["branchCode"][0].ToString();

                }

                if (myResultPropColl.Contains("employeeID"))
                {
                    UserData.personEmployeeID = theSearchResult.Properties["employeeID"][0].ToString();

                } //end ifemployeeID


                if (myResultPropColl.Contains("jpegPhoto"))
                {
                    UserData.personimage = (byte[])theSearchResult.Properties["jpegPhoto"][0];

                } //end jpeg photo

                if (myResultPropColl.Contains("extensionAttribute5"))
                {
                    UserData.persongrade = theSearchResult.Properties["extensionAttribute5"][0].ToString();

                }//grade level
                if (myResultPropColl.Contains("extensionAttribute3"))
                {
                    UserData.personaccountnumber = theSearchResult.Properties["extensionAttribute3"][0].ToString();

                }//account number
                if (myResultPropColl.Contains("extensionAttribute14"))
                {
                    UserData.persongender = theSearchResult.Properties["extensionAttribute14"][0].ToString();

                }//gender
                if (myResultPropColl.Contains("extensionAttribute12"))
                {
                    UserData.personhouseaddress = theSearchResult.Properties["extensionAttribute12"][0].ToString();

                }//house address
                if (myResultPropColl.Contains("extensionAttribute2"))
                {
                    UserData.miscode = theSearchResult.Properties["extensionAttribute2"][0].ToString();

                }

                if (myResultPropColl.Contains("extensionAttribute13"))
                {
                    UserData.personnationality = theSearchResult.Properties["extensionAttribute13"][0].ToString();

                }//nationality
                if (myResultPropColl.Contains("extensionAttribute7"))
                {
                    UserData.persondateofbirth = theSearchResult.Properties["extensionAttribute7"][0].ToString();

                }//date of birth
                if (myResultPropColl.Contains("extensionAttribute15"))
                {
                    UserData.personmaritalstatus = theSearchResult.Properties["extensionAttribute15"][0].ToString();

                }//marital status
                if (myResultPropColl.Contains("extensionAttribute6"))
                {
                    UserData.personemploymentdate = theSearchResult.Properties["extensionAttribute6"][0].ToString();

                }
                if (myResultPropColl.Contains("extensionAttribute9"))
                {
                    UserData.personBranchCode = theSearchResult.Properties["extensionAttribute9"][0].ToString();

                }
                //Employment date
                if (myResultPropColl.Contains("memberof"))
                {
                    //userData.personmemberof = theSearchResult.Properties["memberof"][0].ToString();
                    UserData.personmemberof = Getmemberof(theSearchResult);

                }


            }
            else
            {
                _errorMessage += "User does not exist.";
            } //end if
        } //end PopulateUserDataStruct

        #endregion

        #region class properties

        /// <summary>
        /// LDAP connection string property.
        /// </summary>
        public string LdapConnectionString
        {
            get { return LDapConnectionString; }
            set { LDapConnectionString = value; }
        } //end LDAPConnectionString property

        /// <summary>
        /// LDAP server login username property
        /// </summary>
        public string LdapServerLoginUsername
        {
            get { return LDapServerLoginUsername; }
            set { LDapServerLoginUsername = value; }
        } //end LDAPServerLoginUsername property

        /// <summary>
        /// LDAP server login password property
        /// </summary>
        public string LdapServerLoginPassword
        {
            get { return LDapServerLoginPassword; }
            set { LDapServerLoginPassword = value; }
        } //end LDAPServerLoginPassword property

        /// <summary>
        /// Error message property. Manages any errors that may occur when an instance
        /// of this class is being used.
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        } //end ErrorMessage property

        public string TheUserToLookup
        {
            set { UserToLookup = value; }
            get { return UserToLookup; }
        }


        #endregion

        #region init user details

        /// <summary>
        /// Initialise the user data struct to non-null values - very safe!!
        /// </summary>
        public void InitUserDetails()
        {
            UserData.personFirstName = string.Empty;
            UserData.personSurname = string.Empty;
            UserData.personJobTitle = string.Empty;
            UserData.personJobDescription = string.Empty;
            UserData.personAddress1 = string.Empty;
            UserData.personTown = string.Empty;
            UserData.personCounty = string.Empty;
            UserData.personPostCode = string.Empty;
            UserData.personLocation = string.Empty;
            UserData.personTelephone = string.Empty;
            UserData.personMobileNumber = string.Empty;
            UserData.personEmail = string.Empty;
            UserData.personDepartment = string.Empty;
            UserData.personDisplayname = string.Empty;//displayName
            UserData.personDescription = string.Empty;//description
            UserData.personOtherTelephone = string.Empty;//otherTelephone
            UserData.personWwwHomePage = string.Empty;//wWWHomePage
            UserData.personUrl = string.Empty;//url
            UserData.personPostOfficeBox = string.Empty;//postOfficeBox
            UserData.personCountryCode = string.Empty;//countryCode
            UserData.personUserPrincipalName = string.Empty;//userPrincipalName
            UserData.personSAMAccountName = string.Empty;//sAMAccountName
            UserData.personHomePhone = string.Empty;//homePhone
            UserData.personOtherHomePhone = string.Empty;//otherHomePhone
            UserData.personPager = string.Empty;//pager
            UserData.personOtherPager = string.Empty;//otherPager
            UserData.personOtherMobile = string.Empty;//otherMobile
            UserData.personFax = string.Empty;//facsimileTelephoneNumber
            UserData.personOtherFax = string.Empty;//otherFacsimileTelephoneNumber
            UserData.personIpPhone = string.Empty;//ipPhone
            UserData.personOtherIpPhone = string.Empty;//otherIpPhone
            UserData.personCompany = string.Empty;//company
            UserData.personManager = string.Empty;//manager
            UserData.personEmployeeID = string.Empty;//employeeID
            UserData.personCountry = string.Empty;//co
            UserData.personMiddlename = string.Empty;//middlename
            UserData.personimage = null;
            UserData.persongrade = string.Empty;
            UserData.personaccountnumber = string.Empty;
            UserData.persongender = string.Empty;
            UserData.personofficeaddress = string.Empty;
            UserData.personnationality = string.Empty;
            UserData.personhouseaddress = string.Empty;
            UserData.persondateofbirth = string.Empty;
            UserData.personmaritalstatus = string.Empty;
            UserData.personemploymentdate = string.Empty;
            UserData.personmemberof = new List<string>();
            UserData.personBranchCode = string.Empty;
            UserData.personLevel = string.Empty;


        } //end InitUserDetails

        #endregion

        #region user data structure

        /// <summary>
        /// Convinient structure to hold LDAP users information
        /// Simply contains fields
        /// </summary>
        public struct ApplicationUserData
        {
            public string personFirstName; //givenName
            public string personSurname; //sn                       
            public string personJobTitle; //title
            public string personJobDescription; //info                      
            public string personAddress1; //streetAddress
            public string personTown; //l
            public string personCounty; //st
            public string personPostCode;//postalCode
            public string personLocation; //physicalDeliveryOfficeName 
            public string personTelephone; //telephoneNumber
            public string personMobileNumber; //mobile
            public string personEmail; //mail
            public string personDepartment; //department
            public string personDisplayname; //displayName
            public string personDescription; //description
            public string personOtherTelephone; //otherTelephone
            public string personWwwHomePage; //wWWHomePage
            public string personUrl; //url
            public string personPostOfficeBox; //postOfficeBox
            public string personCountryCode; //countryCode
            public string personUserPrincipalName; //userPrincipalName
            public string personSAMAccountName; //sAMAccountName
            public string personHomePhone; //homePhone
            public string personOtherHomePhone; //otherHomePhone
            public string personPager; //pager
            public string personOtherPager; //otherPager
            public string personOtherMobile; //otherMobile
            public string personFax; //facsimileTelephoneNumber
            public string personOtherFax; //otherFacsimileTelephoneNumber
            public string personIpPhone; //ipPhone
            public string personOtherIpPhone; //otherIpPhone
            public string personCompany;//company
            public string personManager; //manager
            public string personEmployeeID; //employeeID
            public string personCountry; //co
            public string personMiddlename; //middlename
            public byte[] personimage;
            public string persongrade;
            public string personaccountnumber;
            public string persongender;
            public string personofficeaddress;
            public string personnationality;
            public string personhouseaddress;
            public string persondateofbirth;
            public string personmaritalstatus;
            public string personemploymentdate;
            public string miscode;
            public List<string> personmemberof;
            public string personBranchCode;
            public string personLevel;
        } //end ApplicationUserData struct.

        #endregion

        #region unit testing


        /// <summary>
        ///  Main method for testing. You need to compile this class into an EXE first and then
        /// invoke that EXE from a command-line in order to run the tests. 
        /// </summary>          
        //public static void Main()
        //{
        //    ActiveDirectoryWrapper adWrapper = new ActiveDirectoryWrapper();

        //    //optionally pass in parameters from the command line
        //    //if (args.Length != 0)
        //    //{
        //    //    //assing them to properties as you see fit
        //    //} //end if

        //    adWrapper.PopulateUserDataStruct(adWrapper.GetUserDirectoryEntryDetails("olajideolu"));

        //    Console.WriteLine("\n\nPerson name is  " + adWrapper.userData.personFirstName);
        //    Console.WriteLine("Surname is " + adWrapper.userData.personSurname);
        //    Console.WriteLine("Email is " + adWrapper.userData.personEmail);
        //    Console.WriteLine("Person job title is " + adWrapper.userData.personJobTitle);
        //    Console.WriteLine("Person Mobile is " + adWrapper.userData.personMobileNumber);
        //    Console.WriteLine("Person Company is " + adWrapper.userData.personCompany);
        //    Console.WriteLine("Person SAM Account is " + adWrapper.userData.personSAMAccountName);
        //    Console.ReadLine();


        //    if (adWrapper.ErrorMessage != string.Empty)
        //    {
        //        Console.WriteLine(adWrapper.ErrorMessage);
        //        Console.ReadLine();
        //    }//end if

        //} //end main


        #endregion

        #endregion

        public bool authlogindetails(string username, string password)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "accessbankplc.com"))
            {
                bool isValid = pc.ValidateCredentials(username, password);
                return isValid;
            }
        }
    }

}
