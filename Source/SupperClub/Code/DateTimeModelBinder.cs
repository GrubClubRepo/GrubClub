using System.Web.Mvc;
using System;
using System.Globalization;
using log4net;

public class DateTimeBinder : IModelBinder
{
    protected static readonly ILog log = LogManager.GetLogger(typeof(DateTimeBinder));

    public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {       

        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (value == null)
            return null;
        else
        {
            try
            {
                var date = value.ConvertTo(typeof(DateTime), CultureInfo.CurrentCulture);
                return date;
            }
            catch (Exception ex)
            {
                log.Error("Error Converting datetime. " + ex.Message + " Date:" + value + " CurrentCulture:" + CultureInfo.CurrentCulture + " Model Name:"+ bindingContext.ModelName );
                return null;
            }
        }
    }
}