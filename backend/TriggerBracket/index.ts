import { AzureFunction, Context, HttpRequest } from "@azure/functions";
import PhoneNumbers from "../phone_numbers";

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {
    context.log('HTTP trigger function processed a request.');
    const name = (req.query.name || (req.body && req.body.name));

    if (name && PhoneNumbers[name]) {
        context.res = {
            // status: 200, /* Defaults to 200 */
            body: "Hello " + (req.query.name || req.body.name)
        };

        context.bindings.message = [{
            body : "ay boy boy",
            to : PhoneNumbers[name]
        }];
    }
    else {
        context.res = {
            status: 400,
            body: "Please pass a name on the query string or in the request body"
        };
    }
};

export default httpTrigger;
