Un Tenant (que en español se traduce literalmente como "inquilino") es el término que se usa en arquitectura de software para referirse a cada  
 Empresa, Cliente o Heladería que contrata y utiliza tu ERP SaaS.  
 ──────

### 🏢 La Analogía del Edificio de Departamentos

Imagina que tu ERP es un edificio de departamentos:

• El Edificio completo: Es tu software ERP (mismo código, mismos servidores, misma base de datos).  
 • Cada Departamento (Tenant): Es una Heladería distinta que te paga una suscripción mensual (ejemplo: Heladería Cremas del Sur, Heladería Grido,
Heladería Artesanal Don Luis).  
 • La Llave del Departamento: Es el TenantId (el ID único de la empresa).

Cada "inquilino" entra a su propio departamento con su llave. Aunque todos comparten la estructura física del edificio (servidores), ningún  
 inquilino puede mirar ni tocar lo que hay dentro del departamento del vecino.  
 ──────

### 🛡️ ¿Por qué es la pieza más crítica de nuestro ERP?

En un ERP Multi-Tenant (Multi-Empresa):

1. Un solo backend y base de datos sirven a cientos de heladerías. Esto abarata muchísimo tus costos de servidor.
2. Aislamiento Total de Datos: Si el dueño de Heladería Don Luis inicia sesión, la API debe garantizar que solo pueda ver las recetas, insumos,  
   precios y lotes de su heladería.
3. ¿Qué pasa si falla el aislamiento? Si un error en la base de datos permite que la Heladería A vea las recetas secretas de costo o los clientes
   de la Heladería B, el negocio se destruye por una brecha de seguridad grave.  


Por eso, en nuestro sistema:

• Cada registro en la base de datos (un sabor, una materia prima, una carapina) tendrá grabado el TenantId de la empresa a la que pertenece.  
 • Entity Framework Core filtrará automáticamente los datos por el TenantId de la sesión del usuario conectado.  
 ──────

### 📌 En Resumen

• Tenant = Empresa / Heladería Cliente.  
 • TenantId = El identificador único de esa heladería.
