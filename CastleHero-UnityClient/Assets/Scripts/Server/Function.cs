using System;
using LitJson;
using Newtonsoft.Json.Linq;


namespace BackendFunction
{
    /// <summary>
    /// 특정 함수를 호출 시 차트 아이디가 들어가 있는 부분은
    /// 뒤끝 베이스 호출 시 소요되는 시간을 최대한 배제하기 위해
    /// 클라이언트에서 차트 번호 확인 후 인자로 넘겨주는 방식으로 구현.
    /// </summary>
    public partial class BFunc
    {
        public JsonData Invoke(JsonData jsonData)
        {
            if (!jsonData.ContainsKey("functionName"))
                return null;

            return JsonMapper.ToObject(Execution(jsonData));
        }

        private string Execution(JsonData jsonData)
        {
            var functionName = jsonData["functionName"].ToString();
            if (functionName == "DefaultData")
                return SetDefaultUserData();

            var parameters = jsonData["parameters"];
            Func<string> method = null;
            switch (functionName)
            {
                case "StageClear":
                    method = () =>
                        StageClear(
                            parameters[0].ToInt(),
                            parameters[1].ToInt(),
                            parameters[2].ToInt(),
                            parameters[3].ToInt(),
                            parameters[4].ToInt()
                        );
                    break;
                case "OpenBox":
                    method = () =>
                        OpenBox(
                            parameters[0].ToInt(),
                            parameters[1].ToInt(),
                            parameters[2].ToInt()
                        );

                    break;
                case "SummonX1":
                    method = () =>
                        SummonX1(
                            parameters[0].ToInt(),
                            parameters[1].ToInt(),
                            parameters[2].ToString(),
                            parameters[3].ToString()
                        );

                    break;
                case "SummonX10":
                    method = () =>
                        SummonX10(
                            parameters[0].ToInt(),
                            parameters[1].ToInt(),
                            parameters[2].ToString(),
                            parameters[3].ToString()
                        );

                    break;
                case "LevelUp":
                    method = () =>
                        LevelUp(
                            parameters[0].ToInt(),
                            parameters[1].ToInt(),
                            parameters[2].ToInt(),
                            parameters[3].ToInt(),
                            parameters[4].ToInt()
                        );

                    break;
                case "Upgrade":
                    method = () =>
                        Upgrade(
                            parameters[0].ToInt(),
                            parameters[1].ToInt(),
                            parameters[2].ToInt(),
                            parameters[3].ToInt(),
                            parameters[4].ToInt()
                        );

                    break;
            }
            
            return method != null ? method.Invoke() : string.Empty;
        }

        static string ReturnErrorObject(string err)
        {
            JObject error = new JObject();
            error.Add("error", err);

            return error.ToString();
        }

        static string ReturnInvalidRequest() =>
            ReturnErrorObject(ErrorCode.InvalidRequest.ToString());
    }
}