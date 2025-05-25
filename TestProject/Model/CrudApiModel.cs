namespace TestProject.Model
{
    public class CrudApiModel
    {
        public class CreateMode
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Gender { get; set; }
        }
        public class ApiCommonResponseModel
        {
            public int status { get; set; }
            public string message { get; set; }
        }
        public class  getUserDataModel : CreateMode
        {
            public int Id { get; set; }
            public string message { get; set; }
        }
    }
}
