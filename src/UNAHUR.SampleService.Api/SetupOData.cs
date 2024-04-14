namespace UNAHUR.SampleService.Api;

using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;



internal class SetupOData
{
    public static IEdmModel GetEdmModel_V1()
    {

        ODataConventionModelBuilder builder = new();

        /* 
         * IMPORTANTE:CADA CONTROLADOR REGISTRADO TIEN AddDefaultPageSize 
         * SE PUEDE REMOVER PERO SE DEBE ESPECIFICAR UN TAMAÑO DE PAGINA RAZONABLE 
         * PARA EVITAR UNA DENEGACION DE SERVICIO
        */

        /* builder.EntitySet<GrantorsBatchesInfo, GrantorsBatchesInfoController>()
             .EntityType
             .HasKey(e => e.BatchId)
             .AddDefaultPageSize();
        */

        return builder.GetEdmModel();
    }
}
/// <summary>
/// Clase que define extenciones para la configuracion de odata
/// </summary>
static class ODataExtensionOperations
{
    /// <summary>
    /// Tamaño de pagina
    /// </summary>
    const int MAX_TOP = 50;


    /// <summary>
    /// Agrega el tamaño por default en la entidad
    /// </summary>
    /// <typeparam name="T">Tipo a configurar</typeparam>
    /// <param name="conf">Configuracion</param>
    /// <returns></returns>
    public static StructuralTypeConfiguration<T> AddDefaultPageSize<T>(
        this StructuralTypeConfiguration<T> conf) where T : class
    {
        return conf.Page(MAX_TOP, MAX_TOP);
    }

    /// <summary>
    /// Registra un controlador odata a un tipo especifico en la ruta odata con el nombre del controlador sin la palabra "Controller"
    /// </summary>
    /// <typeparam name="TEntityType">Tipo de dato de la entidad</typeparam>
    /// <typeparam name="TControllerType">Tipo de dato del controlados</typeparam>
    /// <param name="builder"></param>
    /// <param name="addDefaultPageSize">Agrega las restricciones de pagina por defecto</param>
    /// <returns></returns>
    public static EntitySetConfiguration<TEntityType> EntitySet<TEntityType, TControllerType>(this ODataModelBuilder builder, bool addDefaultPageSize = true)
        where TEntityType : class
        where TControllerType : ODataController
    {

        var name = typeof(TControllerType).Name.Replace("Controller", "");
        var ret = builder.EntitySet<TEntityType>(name);

        if (addDefaultPageSize)
            ret.EntityType.AddDefaultPageSize();

        return ret;

    }
}
