using System.Collections.Generic;
using System.DirectoryServices;
using static OnlineAD.Api.Domain.ActiveDirectoryService;

namespace OnlineAD.Api.Domain
{
    public interface IActiveDirectoryService
    {
        SearchResult GetUserDirectoryEntryDetails(string loginUserName);
        List<string> Getmemberof(SearchResult theSearchResult);
        string GetMemberFullname(string userName);
        void PopulateUserDataStruct(SearchResult theSearchResult);
        bool authlogindetails(string username, string password);
        ApplicationUserData GetUserData(string userName);

    }
}