namespace GelatoERP.Domain.Enums
{
    public enum UserRoleType
    {
        SuperAdmin = 1,        // Administrador global de la plataforma SaaS                                                                   
        TenantAdmin = 2,       // Dueño / Gerente General de la Heladería                                                                          
        ProductionManager = 3, // Jefe de Planta / Maestro Heladero / Pasteurizador                                                                
        LogisticsManager = 4,  // Encargado de Despacho, Reparto y Carapinas                                                                       
        PointOfSaleUser = 5,   // Operario de Sucursal / Mostrador                                                                                 
        QualityInspector = 6   // Técnico de Laboratorio / Control de Calidad  
    }
}