using static TestProject.Model.CrudApiModel;

namespace TestProject.Services.CrudApiService
{
    public interface ICrudApiService
    {
        Task<ApiCommonResponseModel> USP_CREATE_INS(object Parameters);
        Task<getUserDataModel> USP_USERDATA_GET(object Parameters);
        Task<ApiCommonResponseModel> USP_DELETE_BY_ID(object Parameters);

        
    }
}
