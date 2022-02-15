# ATI.Gaidai

Сервис для создания сценариев - набора http запросов, выполняющихся вместе.


Чтобы создать свой сценарий вам нужно добавить (или использовать уже добавленные) сущности:
####1) Сервис в файле servicessettings.json или Consul KV gaidai/servicessettings
```json
{
      "Id": "ServiceName",                                             
      "Description": "Service description",                            
      "Path": "http://staging.service-name.service.consul:5013/",      
      "AdditionalHeaders": {                                           
        "X-some-Header": "header value"
      },
      "TransitHeaders": [
       {
         "Name": "X-some-Header2",
         "Required" : true
       }
      ],
      "AdditionalHCookies": {                                           
        "X-some-cookie": "cookie value"
      },
      "TransitCookies": [
       {
         "Name": "X-some-cookie2",
         "Required" : true
       }
      ]
}
```
```Id``` - Идентификатор сервиса

```Description``` - описание *(опционально)*

```Path``` - Путь к сервису

```AdditionalHeaders ```- Дополнительные заголовки, которые будут передаваться при каждом запросе *(опционально)*

```TransitHeaders ```- Заголовки из запроса к сервису сценариев, которые будут передаваться при каждом запросе *(опционально)*

```AdditionalHCookies ```- Дополнительные cookie, которые будут передаваться при каждом запросе *(опционально)*

```TransitCookies ```- Cookie из запроса к сервису сценариев, которые будут передаваться при каждом запросе *(опционально)*

####2) Метод в файле methodssettings.json или Consul KV gaidai/methodssettings
```json
{
      "Id": "FirstMethod",
      "Description": "Method Description",
      "ServiceId": "ServiceName",
      "Url": "v1/service/method/{0}",
      "HttpMethod": "Get",
      "Parameters": [
        {
          "Name": "param_first",
          "Required": true,
          "Destination": "Url",
          "UrlPosition": 0,
          "SourceMethod": "SecondMethod",
          "UnnamedBody": true
        }
      ],
      "ErrorReasonField": "error",
      "ResponseFieldTransformation": {
        "field_first": "transformed_field_first",
        "field_second.field": "transformed_field.transformed_field_second.field"
      },
      "AdditionalHeaders": {                                           
        "X-some-Header": "header value"
      },
      "TransitHeaders": [
       {
         "Name": "X-some-Header2",
         "Required" : true
       }
      ],
      "AdditionalHCookies": {                                           
        "X-some-cookie": "cookie value"
      },
      "TransitCookies": [
       {
         "Name": "X-some-cookie2",
         "Required" : true
       }
      ]
    }
```
```Id``` - Идентификатор метода

```Description``` - описание *(опционально)*

```ServiceId``` - Идентификатор сервиса, которому принадлежит метод

```Url``` - Путь метода

```HttpMethod``` - Http метод 

```Parameters``` - Параметры метода 
   - ```Name``` - Имя параметра
   - ```Required``` - если true то параметр обязательный
   - ```Destination``` - [Url, Body, Header] Куда в запросе нужно вставить параметр
   - ```UrlPosition```- Порядок параметра в Url (при Destination = Url)
   - ```SourceMethod``` - Если указано - имя метода из ответа которого следует взять значение параметра, если не указано значения параметров берутся из запроса на выполнение сценария
   - ```UnnamedBody```- если true - параметр будет вставлен в тело запроса без именования, не в виде Json
   
```ErrorReasonField``` - указывает в каком поле в ответе следует взят информацию об ошибке

```ResponseFieldTransformation``` - правила переименования полей ответа для возврата пользователю, поддерживает вложенность (через точку)

```AdditionalHeaders ```- Дополнительные заголовки, которые будут передаваться при каждом запросе *(опционально)*

```TransitHeaders ```- Заголовки из запроса к сервису сценариев, которые будут передаваться при каждом запросе *(опционально)*

```AdditionalHCookies ```- Дополнительные cookie, которые будут передаваться при каждом запросе *(опционально)*

```TransitCookies ```- Cookie из запроса к сервису сценариев, которые будут передаваться при каждом запросе *(опционально)*

####3) Сценарий в файле scenariosssettings.json или Consul KV gaidai/scenariossettings
```json
    {
      "Id": "FirstScenario",
      "Description": "Scenario description",
      "Methods": [ "FirstMethod", "SecondMethod" ],
      "ResponseMethods": [ "FirstMethod", "SecondMethod" ],
      "IsRewritableResponse": true,
      "IsDependent": true,
      "IsForgettable": false,
      "IsCommonSuccessful": true,
      "ResponseFieldTransformation": {
        "field_first": "transformed_field_first",
        "field_second.field": "transformed_field.transformed_field_second.field"
      }
    }
```
```Id``` - Идентификатор сценария

```Description``` - описание *(опционально)*

```Methods``` - Методы, которые следует выполнить в сценарии

```ResponseMethods``` - Методы из Methods, тело ответа которых следует включить в результат выполнения сценария

```IsRewritableResponse``` - Если true - результат методов будет помещен в корень ответа, повторяющиеся поля перезаписаны по порядку ResponseMethods, если false результаты методов будут помещены в объект по имени метода, на уровень ниже

```IsDependent``` - Если true методы в сценарии будут выполнятся последовательно, если false -  параллельно

```IsForgettable``` - Если true - не будет ожидания выполнения методов

```IsCommonSuccessful``` - если true - при любом упавшем методе в сценарии сценарий будет считаться упавшим, если false - результатом выполнения сценария будет результат последнего метода

```ResponseFieldTransformation``` - правила переименования полей ответа для возврата пользователю, поддерживает вложенность (через точку)



####Выполнение сценария

POST v1/scenarios/{scenarioId}

С именованными параметрами для методов сценария в теле запроса в виде словаря