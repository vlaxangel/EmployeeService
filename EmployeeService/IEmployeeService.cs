using System.ServiceModel;
using System.ServiceModel.Web;
using EmployeeService.Models;

namespace EmployeeService
{
    [ServiceContract]
    public interface IEmployeeService
    {

        [OperationContract]
        [WebGet(UriTemplate = "Employees/{id}",
            ResponseFormat = WebMessageFormat.Json)]
        Employee GetEmployeeById(string id);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "Employees/{id}/Enable?enable={enable}", 
            ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        void EnableEmployee(string id, bool enable);
    }

	
}
