﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using ClickFactura_WebServiceCF.Conectores.Configuracion;
using ClickFactura_Entidades.BD.Entidades;
using ClickFactura_Facturacion.Clases;
using System.Data;

//Views relacionados
//Shared
// _cargaFacturaXML.cshtml

namespace MVC5_full_version.Controllers.FacturasXML
{
    public class facturasXMLController : Controller
    {
        ClickFactura_WebServiceCF.Service.Service1 facturacion = new ClickFactura_WebServiceCF.Service.Service1();
         
        static Dictionary<string, string> FacturasCargadas = new Dictionary<string, string>();
        static List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura> FacturasLeidas = new List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura>();
        //static ServiceFacturacion.objLeidoFactura[] FacturasLeidas;
        static List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objNotaCredito> NotasC = new List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objNotaCredito>();
        static Dictionary<string, string> ComplementosCargados = new Dictionary<string, string>();
        //static List<Clases.Genericos.obj_facturaAPasivo> FacturasAPasivos = new List<Clases.Genericos.obj_facturaAPasivo>();
        static List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.obj_facturaAPasivo> FacturasAPasivos = new List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.obj_facturaAPasivo>();
        //static ClickFactura_Entidades.BD.Entidades.T_objLeidoFactura FacturasLeidas;
        static List<string> elementos = new List<string>();
        string tipo = "";
        bool admin = false;
        static int _subproc = 0;
       // static List<ClickFactura_Facturacion.Clases.paraVerificacionFactura.objPasivoFinanciero> DetallesPF = new List<ClickFactura_Facturacion.Clases.paraVerificacionFactura.objPasivoFinanciero>();
        List<List<string>>lsMensajes;

        #region        Carga y Validación Factura XML
        [HttpPost]
        public PartialViewResult Load()
        {
            return PartialView("_cargaFacturaXML");
        }

        [HttpPost]
        public JsonResult SubirFactura(HttpPostedFileBase[] files)
        {
            bool admin;
            try
            {
                ClickFactura_Facturacion.Clases.paraVerificacionFactura.cs_Factura csfactura = new ClickFactura_Facturacion.Clases.paraVerificacionFactura.cs_Factura();
                bool carga = false;
                string rutaCompleta = string.Empty;
                string nombreArchivo = string.Empty;
                List<string> datos=new List<string>();
                string mensaje = string.Empty;
                string Mes = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
                //admin = Convert.ToBoolean(Session["NivelAdministradorBafar"]);
                string usuario = System.Web.HttpContext.Current.Session["Usuario"] as string;
                admin = ClickFactura_WebServiceCF.Service.Clases.cs_Estaticos.EsAdministradorBafar(usuario);
                if(admin==false)
                {
                    admin = ClickFactura_WebServiceCF.Service.Clases.cs_Estaticos.EsUsuarioInterno(usuario);
                }
                String rfc = "";
                String proveedor = "";
                rfc = Session["Usuario"] == null ? "AAA010101AAA" : Session["Usuario"].ToString();
                datos.Add(rfc);
                string oc = "";
                try
                {
                        oc = Session["OrdenCompra"].ToString();
                }
                catch
                {
                       oc=System.Web.HttpContext.Current.Session["OrdenCompra"] as String;
                }

                if (!admin)
                {
                    proveedor = Session["Num_Proveedor"].ToString();
                    datos.Add(proveedor);
                }
                else
                {
                    proveedor = Session["Num_Proveedor"].ToString();//--->  facturacion.ObtenerNumProveedor(Session["OrdenCompra"].ToString());
                    datos.Add(proveedor);
                }
                datos.Add(Mes.ToUpper());
                datos.Add(Session["OrdenCompra"].ToString());
                List<string> mensajes = new List<string>();
                foreach (var file in files)
                {
                    var archivo = file;
                    if (archivo != null && archivo.ContentLength > 0)
                    {
                        var stream = archivo.InputStream;
                        nombreArchivo = archivo.FileName;
                        if (archivo.ContentType.Contains("zip") || archivo.ContentType.Contains("rar"))
                        {
                            //List<string> dats = datos.ToList();
                            string rutaZip = ClickFactura_Facturacion.Clases.cs_Estaticos.GenerarRuta(datos, nombreArchivo);
                            archivo.SaveAs(rutaZip);
                            //List<ClickFactura_Facturacion.Clases.paraVerificacionFactura.obj_facturaAPasivo>
                            //ServiceFacturacion.objLeidoFactura[] des = facturacion.DescomprimirPaquete(oc, rutaZip, nombreArchivo, datos, out lsMensajes);//oc, rutaZip, nombreArchivo, datos, out lsMensajes);
                            List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura> des = facturacion.factXML_DescomprimirPaquete(oc, rutaZip, nombreArchivo, datos, out lsMensajes);//oc, rutaZip, nombreArchivo, datos, out lsMensajes);
                            foreach (ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura item in des)
                            {
                                var existe = (from t in FacturasCargadas where t.Key.Equals(item.Archivo) select t.Key).FirstOrDefault();
                                if (existe == null)
                                {
                                    FacturasCargadas.Add(item.Archivo, item.Path);
                                }
                            }
                            //List<ClickFactura_Facturacion.Clases.paraVerificacionFactura.obj_facturaAPasivo>
                            //ServiceFacturacion.objLeidoFactura[] facturas = facturacion.FusionarFacturas(des,FacturasLeidas);
                            List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura> facturas = facturacion.factXML_FusionarFacturas(des, FacturasLeidas);
                            //FacturasLeidas =facturacion.FusionarFacturas(des, FacturasLeidas);
                            var encontrados = from anteriores in FacturasLeidas where anteriores.NombreArchivo.Equals(archivo) == true select anteriores;
                            if(encontrados == null)
                                     FacturasLeidas = facturas;
                            Session["FacturaValidada"] = true;
                            Session["MensajesFactura"] = lsMensajes;
                        }
                        else
                        {
                             datos.Insert(0, archivo.ContentType.Contains("xml") ? "XML" : "PDF");
                             //datos[5]=(archivo.ContentType.Contains("xml") ? "XML" : "PDF");
                            //rutaCompleta = ClickFactura_Facturacion.Clases.cs_Estaticos.GenerarRuta(csfactura.convierteaListaStrings(datos), nombreArchivo);
                             rutaCompleta = ClickFactura_Facturacion.Clases.cs_Estaticos.GenerarRuta(datos, nombreArchivo);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            archivo.SaveAs(rutaCompleta);

                            carga = true;
                            string rfcF = "";

                            List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura> v = facturacion.factXML_ValidarFactura(oc, rutaCompleta, nombreArchivo, datos, out rfcF, out mensajes);
                            List<String> mensajesSalida=new List<string>();
                            mensajesSalida = mensajes;// csfactura.convierteaListaStrings(mensajes);
                            if(Session["FacturasLeidas"] ==null)
                                             Session["FacturasLeidas"] = v;
                            else
                            {
                                List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura> Guardados = (List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura>)Session["FacturasLeidas"];
                                List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura> XMLActuales = new List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura>();
                                List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura> XMLnuevos = new List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura>();
                                foreach (var xmlprevio in Guardados )
                                   {
                                        bool encontrado = false;
                                        ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura dePaso = new ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura();
                                        foreach (var xmlnuevo in v)
                                        {
                                            dePaso = xmlnuevo;
                                            if(xmlnuevo.NombreArchivo.Equals(xmlprevio.NombreArchivo)==true)
                                            {
                                                encontrado = true;
                                                break;
                                            }
                                        }
                                        if (encontrado == false)
                                            XMLnuevos.Add(dePaso);
                                   }
                                if(XMLnuevos.Count()>0)
                                      Session["FacturasLeidas"] = XMLnuevos;
                            }
                            mensajesSalida.Insert(0, nombreArchivo);
                            List<String> lsMensajesSalida=new List<string>();

                            //if (mensajesSalida.Count > 0)
                            //{
                            //    foreach (var msg in lsMensajes)
                            //    {
                            //        if (msg[0] == mensajesSalida[0])
                            //        {
                            //            //Pendiente correguir 17 Mayo 2017 GRD son los mensjaes de Error
                            //            //lsMensajes.Remove(msg);
                            //            //lsMensajes.Add(mensajes);
                            //            break;
                            //        }
                            //        else
                            //        {
                            //            lsMensajes.Add(mensajes);
                            //            break;
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    lsMensajes.Add(mensajes);
                            //}

                            if (v != null)
                            {
                                List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura> FacturasLeidas_limpia = FacturasLeidas.Except(FacturasLeidas.GroupBy(i=>i.NombreArchivo).Select(ss => ss.FirstOrDefault())).ToList();
                                FacturasLeidas = FacturasLeidas_limpia;
                                List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objLeidoFactura> facturas = facturacion.factXML_FusionarFacturas(v, FacturasLeidas);
                                var encontrados = from anteriores in FacturasLeidas where anteriores.NombreArchivo.Equals(archivo.FileName) == true select anteriores;
                                if (encontrados == null)
                                {
                                    FacturasLeidas = facturas;
                                    FacturasCargadas.Add(nombreArchivo, rutaCompleta);
                                }
                                else
                                        {
                                            if(encontrados.Count()==1)
                                            {
                                                 FacturasLeidas = facturas;
                                                //Probablemente ya esta cargada
                                                 //FacturasCargadas.Add(nombreArchivo, rutaCompleta);
                                            }
                                            else
                                                {
                                                        var existe = (from t in FacturasCargadas where t.Key.Equals(nombreArchivo)==true select t.Key).FirstOrDefault();
                                                        if (existe == null)
                                                        {
                                                            FacturasCargadas.Add(nombreArchivo, rutaCompleta);
                                                        }
                                                }
                                         }
                            }
                            Session["FacturaValidada"] = carga;
                            Session["MensajesFactura"] = mensajes;// lsMensajes;
                        }
                    }
                }

                return Json(carga, JsonRequestBehavior.AllowGet);
                //Sube VPN
            }
            catch (Exception ex)
            {
                Session["FacturaValidada"] = false;
                Session["MensajesFactura"] = new List<string>();
                return Json("Error en la carga del archivo, favor de intentarlo nuevamente.\nError: " + ex.Message);
            }
        }

        [HttpPost]
        public JsonResult ResultadoFactura()
        {
            try
            {
                bool carga = Session["FacturaValidada"] != null ? Convert.ToBoolean(Session["FacturaValidada"]) : false;
                List<List<string>> lsMensajes = new List<List<string>>();
                List<string> _lsMensajes = new List<string>();
                try
                {
                  lsMensajes = Session["MensajesFactura"] != null ? (List<List<string>>)Session["MensajesFactura"] : new List<List<string>>();
                }
                catch(Exception ex)
                {
                    _lsMensajes = Session["MensajesFactura"] != null ? (List<string>)Session["MensajesFactura"] : new List<string>();
                    lsMensajes.Add(_lsMensajes);
                }
                List<List<string>> mensajes = new List<List<string>>();
                elementos = new List<string>();
                if (carga)
                {
                    string elemento = "";
                    //foreach (var item in lsMensajes)
                    var item = lsMensajes[lsMensajes.Count - 1];
                    {
                        string valor = "";
                        for (int i = 1; i < item.Count; i++)
                        {
                            if (item[i].Substring(0, 1).Equals("0"))
                            {
                                valor = "0";
                                if (item[i].Contains("El sello del emisor es invalido."))
                                {
                                    valor = "2";
                                    item[i] = "2" + item[i].Substring(1);
                                    break;
                                }
                            }
                        }
                        var v = (from t in item select t).LastOrDefault();
                        if (valor == "0")
                        {
                            item.Add(false.ToString());
                            mensajes.Add(item);
                            elemento = "<div class=\"alert alert-danger alert-dismissable panel-body\">"
                                            + "<div class=\"col-md-4\">"
                                                + "<img id=\"icono\" class=\"img-circle\" style=\"width:30px;\" src=\"/Images/bafar/iconNoProcedio.png\" />"
                                                + "<strong> Factura: " + item[0] + "</strong>"
                                            + "</div>"
                                            + "<div class=\"col-md-2\">"
                                                + "<button type=\"button\" class=\"btn btn-danger\" onclick=\"AbrirDetallesMensajes('" + item[0] + "')\"><i class=\"fa  fa-times\"></i>  Detalles</button>"
                                            + "</div>"
                                      + "</div>";
                            elementos.Add(elemento);
                        }
                        else if (valor == "2")
                        {
                            item.Add(true.ToString());
                            mensajes.Add(item);
                            elemento = "<div class=\"alert alert-warning alert-dismissable panel-body\">"
                                            + "<div class=\"col-md-4\">"
                                                + "<img id=\"icono\" class=\"img-circle\" style=\"width:30px;\" src=\"/Images/bafar/iconNoProcedio.png\" />"
                                                + "<strong> Factura: " + item[0] + "</strong>"
                                            + "</div>"
                                            + "<div class=\"col-md-2\">"
                                                + "<button type=\"button\" class=\"btn btn-warning\" onclick=\"AbrirDetallesMensajes('" + item[0] + "')\"><i class=\"fa fa-warning\"></i> Detalles</button>"
                                            + "</div>"
                                      + "</div>";
                            elementos.Add(elemento);
                        }
                        else //if (v != "False")
                        {
                            var nom = (from t in FacturasLeidas where t.Archivo == item[0].Trim() select t).FirstOrDefault();
                            elemento = "<div class=\"alert alert-success alert-dismissable\">"
                                        + "<img id=\"icono\" class=\"img-circle\" style=\"width:30px;\" src=\"/Images/bafar/iconProcedio.png\" />"
                                        + "<strong> Factura Correcta: " + nom.Archivo + "</strong>"
                                   + "</div>";
                            var existe = (from t in elementos where t.Contains(elemento) select t).FirstOrDefault();
                            if (existe == null)
                                elementos.Add(elemento);
                        }

                    }
                    return Json(new
                    {
                        elementos = elementos,
                        mensajes = mensajes,
                    });
                }
                else
                {
                    return Json("La carga no se realizó");
                }
            }
            catch
            {
                return Json("La carga no se realizó");
            }

        }

        [HttpPost]
        public ActionResult CompararImportes(List<ClickFactura_Entidades.BD.Modelos.Registros> registros)
        {
            try
            {
                string mensaje = "";
                registros = ConvertirRegistros(registros);
                List<ClickFactura_Entidades.BD.Modelos.Registros> _Seleccionados =System.Web.HttpContext.Current.Session["TablaRecepcionesAPasivo"] as List<ClickFactura_Entidades.BD.Modelos.Registros>;
                int indice = 0;
                foreach (ClickFactura_Entidades.BD.Modelos.Registros _reg in _Seleccionados)
                {
                   //var result = from destino in registros where destino.R3.Equals(registros[indice].R1) == true && sesion.R2.Equals(registros[indice].R2) == true && sesion.R4.Equals(registros[indice].R3) == true && sesion.R7.Equals(registros[indice].R5) == true select sesion;
                    foreach (ClickFactura_Entidades.BD.Modelos.Registros origen in registros)
                    {
                        if (origen.R1.Equals(_reg.R3) == true)
                            if (origen.R2.Equals(_reg.R2) == true)
                                if (origen.R3.Equals(_reg.R4) == true)
                                    if (origen.R7.Equals(_reg.R7) == true)
                                    {
                                        registros[indice].R10 = _reg.R1;
                                        registros[indice].R18 = _reg.R18;
                                    }
                    }
                    indice++;
                }

                //registros[0].R18 = FacturasLeidas[0].NombreArchivo;
                //Original se desplaza el 31 de Octubre de 2017 GRD
                //var lista = facturacion.factXML_CompararImportes(registros, FacturasLeidas, NotasC, out mensaje);
                //Se activa el 31 de Octubre de 2017
                var lista = facturacion.factXML_CompararImportes(_Seleccionados, FacturasLeidas, NotasC, out mensaje);
                //Session["TablaRecepcionesAPasivo"] = registros;
                string elemento = "";
                List<string> etiquetado = new List<string>();
                bool correcto = false;
                foreach (var item in lista)
                {
                    correcto = Convert.ToBoolean(item.Value);
                    if (correcto)
                        elemento = "<div class=\"alert alert-success alert-dismissable\">";
                    else
                        elemento = "<div class=\"alert alert-danger alert-dismissable\">";
                    var msg = item.Key.Split('|');
                    foreach (var etiqueta in msg)
                    {
                        if (etiqueta != "")
                        {
                            var lbl = etiqueta.Split('*');
                            if (lbl.Length > 1 || etiqueta.Contains(".xml"))
                            {
                                elemento += "<p class=\"text-justify\"><ul>";
                                foreach (var we in lbl)
                                {
                                    elemento += "<li>" + we + "</li>";
                                }
                                elemento += "</ul></p>";
                            }
                            else
                            {
                                elemento += "<p class=\"text-justify\">" + etiqueta + "</p>";
                            }
                        }
                    }
                    elemento += "</div>";
                    etiquetado.Add(elemento);
                }
                return Json(new
                {
                    lista = etiquetado,
                    correcto = correcto
                });
            }
            catch (Exception ex)
            {
                JavaScript("javascript:showErrorMessage('" + "Error interno: " + ex.Message + "');");
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult GenerarPasivo()
        {
            //cor = new ConOrdenyRecepcion();
            string NumeroPasivo = "";
            tipo = System.Web.HttpContext.Current.Session["TipoUsuario"] as string;// Session["TipoUsuario"].ToString();
            bool esExterno = tipo == "Externo" ? true : false;
            //var registros = (List<ClickFactura_Entidades.BD.Modelos.Registros>)Session["TablaRecepcionesAPasivo"];
            var registros = System.Web.HttpContext.Current.Session["TablaRecepcionesAPasivo"] as List<ClickFactura_Entidades.BD.Modelos.Registros>; 
            List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.obj_facturaAPasivo> facturas = new List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.obj_facturaAPasivo>();
            var UUID = new Guid();
            foreach (var item in FacturasLeidas)
            {
                //Se edita el día 9 Agosto GRD
                var reg = (from t in registros where t.R18.Equals(item.NombreArchivo) select t).ToList();
                //var reg = (from t in registros where t.R6.Equals(item.NombreArchivo) select t).ToList();
                UUID = Guid.NewGuid();

                foreach (var r in reg)
                {
                    var fac = (from t in FacturasAPasivos where t.OrdenCompra.Equals(r.R1) && t.NoRecepcion.Equals(r.R2) && t.Posicion.Equals(r.R3) select t).FirstOrDefault();

                    if (fac != null)
                    {
                        #region Por factura
                        fac.AplicaraProrrateo = item.AplicaraProrrateo;
                        fac.Conceptos = item.Conceptos;
                        fac.Descuento = item.Descuento;
                        fac.DiferenciaImportes = item.DiferenciaImportes;
                        fac.FolioFactura = item.Folio;
                        fac.ImporteArticulosconIVA = item.ImporteArticulosconIVA;
                        fac.FechaFactura = item.Fecha;
                        //ImporteNotaCredito = item.NotasCredito;                    
                        fac.ImporteRecepcionOrigen = item.ImporteRecepcionOrigen;
                        fac.ImporteRetencionFlete = item.ImporteRetencionFlete;
                        fac.ImportesFueron = item.ImportesFueron;
                        fac.ImpuestoRetenido = item.ImporteRetenido;
                        fac.Impuestos = item.Impuestos;
                        fac.Impuestos33 = item.Impuestos33;
                        //ItemEstatus = 
                        //IvaTrasladadoTotal =                     
                        fac.Moneda = item.Moneda;
                        fac.NombreArchivoXML = item.NombreArchivo;
                        fac.NotasCredito = item.NotasCredito;
                        fac.Num_Proveedor = System.Web.HttpContext.Current.Session["Num_Proveedor"] as String;//Se debe obtener de pasos previos Num_Proveedor cor.ObtenerNumProveedor(fac.OrdenCompra);
                        fac.Path = item.Path;
                        fac.RecepcionValia = item.RecepcionValia;
                        fac.ReglasAplicadas = item.ReglasAplicadas;
                        //RetencionISRRenta =                     
                        fac.RFC = item.RFC;
                        fac.RFC_Receptor = item.RFC_Receptor;
                        fac.Serie = item.Serie;
                        fac.SubTotalXML = item.SubTotal;
                        fac.TextoXML = item.XmlaTexto;
                        fac.TipoFactura = item.TipoFactura;
                        fac.UUID = item.UUID;
                        //fac.OrdenCompra = r.R10.ToString();
                        facturas.Add(fac);
                        #endregion Por factura
                    }
                }
            }

            List<KeyValuePair<string, string>> resultados = new List<KeyValuePair<string, string>>();
            List<KeyValuePair<string, string>> resultados2 = new List<KeyValuePair<string, string>>();
            KeyValuePair<string, string> _agregar = new KeyValuePair<string, string>("msg1", "Iniciando proceso de Generación de Pasivo.");
            resultados2.Add(_agregar);
            bool res = false;
            try
            {
                //factXML_GenerarPasivo(List<ClickFactura_Entidades.BD.Modelos.Registros> registros, List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.obj_facturaAPasivo> facturas, List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.objNotaCredito> listadoNC, out List<KeyValuePair<string, string>> resultados, bool esExterno, int idUsuario, out string NumeroPasivoGenerado, bool esOrden = true)
                res = facturacion.factXML_GenerarPasivo(registros, facturas, NotasC, out resultados, esExterno, Convert.ToInt32(Session["IdUsuario"]), out NumeroPasivo);
                _agregar = new KeyValuePair<string, string>("msg2", "Proceso de Generar Pasivo finalizado.");
                resultados2.Add(_agregar);
                _agregar = new KeyValuePair<string, string>("msg3", "Resultado: " + resultados.Count);
                resultados2.Add(_agregar);
                _agregar = new KeyValuePair<string, string>("msg4", "Key: " + resultados != null ? resultados[0].Key : "Nada" + " Valor: " + resultados != null ? resultados[0].Value : "Nada");
                resultados2.Add(_agregar);
                if (resultados.Count > 0)
                {
                    foreach (var item in resultados2)
                    {
                        resultados.Add(item);
                        JavaScript("javascript:showMessage('"+lsMensajes+"');");
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            string mensaje="";
            if (NumeroPasivo != "")
            {
                if (NumeroPasivo != null)
                    ActualizarTablasOC(facturas[0].OrdenCompra, NumeroPasivo, out mensaje);
                else
                {
                    res = false;
                }
            }
            string div = "";
            if (res)
            {
                div = "<div class=\"alert alert-success alert-dismissable\">";
                div += "<img id=\"icono\" class=\"img-circle\" style=\"width:30px;\" src=\"/Images/bafar/iconProcedio.png\" />";
                foreach (var item in registros)
                {
                    item.R8 = "iconProcedio.png";
                    item.R9 = "Se generó el pasivo.";
                }
            }
            else
            {
                div = "<div class=\"alert alert-danger alert-dismissable\">";
                div += "<img id=\"icono\" class=\"img-circle\" style=\"width:30px;\" src=\"/Images/bafar/iconNoProcedio.png\" />";
                foreach (var item in registros)
                {
                    item.R8 = "iconNoProcedio.png";
                    item.R9 = "No se generó el pasivo.";
                }
                Session["TablaRecepcionesAPasivo"] = registros;
            }
            foreach (var item in resultados)
            {
                div += item.Value + "</br>";
            }
            div += "</div>";
            return Json(new
            {
                pasivo = res,
                mensaje = div
            });
        }

        public bool ActualizarTablasOC(string oc, string numPasivo, out string mensaje)
        {
            try
            {
                mensaje = "";
                using (Desarrollo_CF contexto = new Desarrollo_CF())
                {
                    var tabla = from b in contexto.Detalle_Recepciones select b;
                    var recepciones = (from t in tabla
                                       where t.Orden_Compra.Equals(oc)
                                       select t).ToList();
                    var detalleOC = (from t in contexto.Detalle_OrdenCompra
                                     where t.Orden_Compra.Equals(oc)
                                     select t).ToList();
                    foreach (var item in recepciones)
                    {
                        item.Numero_ActualImput = "01";
                        item.Clase_Operacion = "2";
                        item.Tipo_Movimiento = "Q";
                        item.Clave_Movimiento = null;
                        contexto.SaveChanges();
                    }
                    foreach (var item in detalleOC)
                    {
                        item.Numero_Pasivo = numPasivo;
                        item.Tipo_Mov = "Q";
                        item.Fecha_Contable = recepciones[0].Fecha_Contable;
                        item.Moneda = recepciones[0].Moneda;
                        item.Doc_Ref = numPasivo;
                        contexto.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        #endregion Carga y Validacion Factura XML

        #region         Operaciones relacionadas a las Orden de Compra
        [HttpPost]
        public ActionResult MostrarTablaDetallesOC()
        {
            if (Session["TablaDetalles"] != null)
            {
                return Json(Session["TablaDetalles"].ToString(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("No se generó la tabla");
            }
        }

        [HttpPost]
        public JsonResult CargarOrdenes()
        {
            #region hardoceado para logear
            Session["TipoUsuario"] = "Externo";// ClickFactura_WebServiceCF.Service.Clases.cs_Estaticos.tipoUsuario("3");
            string quienEntro= System.Web.HttpContext.Current.Session["Usuario"] as string;
            Session["NivelAdministradorBafar"]= ClickFactura_WebServiceCF.Service.Clases.cs_Estaticos.EsAdministradorBafar(quienEntro);
            if((bool)Session["NivelAdministradorBafar"]==false)
            {
                Session["NivelAdministradorBafar"] = ClickFactura_WebServiceCF.Service.Clases.cs_Estaticos.EsUsuarioInterno(quienEntro);
            }
           #endregion hardoceado para logear

           ClickFactura_WebServiceCF.Conectores.Configuracion.csBaseSAPNET csConectoresSAP=new csBaseSAPNET();
            tipo = Session["TipoUsuario"].ToString();
            admin = Convert.ToBoolean(Session["NivelAdministradorBafar"]);
            if (_subproc == 32)
            {
                if (tipo == "Externo" && !admin)
                {
                    List<KeyValuePair<string, string>> datos = new List<KeyValuePair<string, string>>();
                    List<KeyValuePair<string, string>> ordenes = new List<KeyValuePair<string, string>>();
                    DataTable obtenidoSAP = new DataTable();
                    obtenidoSAP = csConectoresSAP.obtenOrdenCompraWeb(Session["Num_Proveedor"].ToString(), ref datos);//cor.CargarListaOrdenes(Session["Num_Proveedor"].ToString(), out mensaje);
                    return Json(obtenidoSAP, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var control = "<input id='txtOC' type='text' class='form-control' placeholder='Orden de Compra'>";
                    return Json(control, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var control = "<input id='txtOC' type='text' class='form-control' placeholder='Orden de Compra'>";//placeholder='No de Ticket'>";
                return Json(control, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CargaInformacionOrden(string oc)
        {
            //FacturasLeidas = new ServiceFacturacion.objLeidoFactura[]();
            //ServiceFacturacion.objLeidoFactura[] FacturasLeidas;
            List<ClickFactura_Facturacion.Clases.paraVerificacionFactura.objLeidoFactura> FacturasLeidas = new List<ClickFactura_Facturacion.Clases.paraVerificacionFactura.objLeidoFactura>();
            ComplementosCargados = new Dictionary<string, string>();
            FacturasCargadas = new Dictionary<string, string>();
            //NotasC = new List<Clases.Genericos.objNotaCredito>();
            //lsMensajes = new string[20][];//
            lsMensajes=new List<List<string>>();
            elementos = new List<string>();
            FacturasAPasivos = new List<ClickFactura_Facturacion.version3_3.Clases33.paraVerificacionFactura33.obj_facturaAPasivo>();//ServiceFacturacion.objLeidoFactura();//new List<Clases.Genericos.obj_facturaAPasivo>();

            if (_subproc == 0)
            {
                if (oc.Length < 10)
                {
                    for (int i = oc.Length; i < 10; i++)
                    {
                        oc = "0" + oc;
                    }
                }
            }
            tipo = "Admon";//Session["TipoUsuario"].ToString();
            admin = true;//Convert.ToBoolean(Session["NivelAdministradorBafar"]);
            string tablaDetalle = "";
            string proveedor = "";
            if (!admin)
            {
                proveedor = Session["Num_Proveedor"].ToString();
            }
            bool esOrden = _subproc == 0 ? false : true;

            bool esTicket = false;//cor.esTicket(oc, out mensaje);
            if (esTicket)
            {
                if (!esOrden)
                {
                    //No puede procesar un ticket en este modulo, vaya a Punto de Venta.
                    return Json("msg:No puede procesar un ticket en este modulo, vaya a Punto de Venta.");
                }
            }
            else
            {
                if (esOrden)
                {
                    //No puede procesar una orden de compra en este modulo, vaya a Orden y Recepcion.
                    return Json("msg:No puede procesar una orden de compra en este modulo, vaya a Orden y Recepcion.");
                }
            }
            {
                var tabla = new ClickFactura_Entidades.BD.Modelos.TablaGeneralModel();
                string mensaje = "";
                string sociedadVerificar = "";
                proveedor =System.Web.HttpContext.Current.Session["Num_Proveedor"] as string;
                if(proveedor.Length<10)
                {
                    for (int i=0; i <= 5;i++ )
                    {
                        proveedor = "0" + proveedor;
                    }
                }
                ClickFactura_WebServiceCF.Service.Service1 serviciooperaciones = new ClickFactura_WebServiceCF.Service.Service1();
                //Limpiando registros anteriores
                Session["FacturasLeidas"] = null;
                //Limpiando registros anteriores
                System.Web.HttpContext.Current.Session["TablaRecepcionesAPasivo"] = null;
                tabla =serviciooperaciones.ListadoOrdenesRecepciones(ref oc, proveedor, tipo, sociedadVerificar,out tablaDetalle, out mensaje,  esOrden, admin);
                Session["OrdenCompra"] = oc;
                Session["TablaDetalles"] = tablaDetalle;
                System.Web.HttpContext.Current.Session["sociedadVerificar"] = sociedadVerificar;
                return PartialView("~/Views/Recepciones/Grid.cshtml", tabla);
            }
        }

        #endregion  Operaciones relacionadas a la Orden de Compra

        #region Carga información de la Tabla de Recepciones

        [HttpPost]
        public ActionResult CargarTablaRecepciones(List<ClickFactura_Entidades.BD.Modelos.Registros> registros, bool finalizar = false)
        {
            try
            {
                List<string> mensajes = new List<string>();
                if (!finalizar)
                {
                    registros = ConvertirRegistros(registros);
                }
                else
                {
                    registros = (List<ClickFactura_Entidades.BD.Modelos.Registros>)Session["TablaRecepcionesAPasivo"];
                }
                var lista = facturacion.viewRecepciones_RecepcionesSeleccionadas(Session["OrdenCompra"].ToString(), ref registros, FacturasLeidas, ref FacturasAPasivos, out mensajes);
                if (lista.Registros!=null)
                       System.Web.HttpContext.Current.Session["TablaRecepcionesAPasivo"] = lista.Registros;
                if (finalizar) 
                    lista.Registros = registros;
                return PartialView("~/Views/Shared/_Grid.cshtml", lista);
                FacturasLeidas = null; 
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        private List<ClickFactura_Entidades.BD.Modelos.Registros> ConvertirRegistros(List<ClickFactura_Entidades.BD.Modelos.Registros> registros)
        {
            try
            {
                foreach (var item in registros)
                {
                    item.R1 = item.R1 != null ? (item.R1 as string[])[0] : "";
                    item.R2 = item.R2 != null ? (item.R2 as string[])[0] : "";
                    item.R3 = item.R3 != null ? (item.R3 as string[])[0] : "";
                    item.R4 = item.R4 != null ? (item.R4 as string[])[0] : "";
                    item.R5 = item.R5 != null ? (item.R5 as string[])[0] : "";
                    item.R6 = item.R6 != null ? (item.R6 as string[])[0] : "";
                    item.R7 = item.R7 != null ? (item.R7 as string[])[0] : "";
                    item.R8 = item.R8 != null ? (item.R8 as string[])[0] : "";
                    item.R9 = item.R9 != null ? (item.R9 as string[])[0] : "";
                    item.R10 = item.R10 != null ? (item.R10 as string[])[0] : "";
                    item.R11 = item.R11 != null ? (item.R11 as string[])[0] : "";
                    item.R12 = item.R12 != null ? (item.R12 as string[])[0] : "";
                    item.R13 = item.R13 != null ? (item.R13 as string[])[0] : "";
                    item.R14 = item.R14 != null ? (item.R14 as string[])[0] : "";
                    item.R15 = item.R15 != null ? (item.R15 as string[])[0] : "";
                    item.R16 = item.R16 != null ? (item.R16 as string[])[0] : "";
                    item.R17 = item.R17 != null ? (item.R17 as string[])[0] : "";
                    item.R18 = item.R18 != null ? (item.R18 as string[])[0] : "";
                    item.R19 = item.R19 != null ? (item.R19 as string[])[0] : "";
                    item.R20 = item.R20 != null ? (item.R20 as string[])[0] : "";
                }
                return registros;
            }
            catch (Exception)
            {
                return new List<ClickFactura_Entidades.BD.Modelos.Registros>();
                throw;
            }
        }

        //public ClickFactura_Entidades.BD.Modelos.TablaGeneralModel RecepcionesSeleccionadas(string ordenCompra, List<ClickFactura_Entidades.BD.Modelos.Registros> registros, ServiceFacturacion.objLeidoFactura[] facturas, ref List<ClickFactura_Facturacion.Clases.paraVerificacionFactura.obj_facturaAPasivo> objFacturasAPasivos, out List<string> mensajes)
        //{
        //    //ServiceFacturacion.objLeidoFactura[]-->ClickFactura_Facturacion.Clases.paraVerificacionFactura.objLeidoFactura
        //    //List<ClickFactura_Facturacion.Clases.paraVerificacionFactura.obj_facturaAPasivo>

        //    mensajes = new List<string>();
        //    try
        //    {
        //        Dictionary<string, Type> columnas = new Dictionary<string, Type>();
        //        columnas.Add("Orden de Compra", typeof(string));
        //        columnas.Add("No Recepción", typeof(string));
        //        columnas.Add("Pos. Recepción", typeof(string));
        //        columnas.Add("Pos. Referencia", typeof(string));
        //        columnas.Add("Pos. Doc. Material", typeof(string));
        //        columnas.Add("Facturas(xml)", typeof(string));
        //        columnas.Add("Importe linea", typeof(double));
        //        columnas.Add("Pasivo", typeof(string));
        //        columnas.Add("Observaciones", typeof(string));
        //        columnas.Add("Impuesto", typeof(string));
        //        string[] template = { "", "", "", "", "", "", "#: kendo.format('{0:c}', R7) #", "<img class=\"img-circle\" style=\"width:30px;\" src=\"/Images/bafar/#:R8#\" />", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
        //        bool[] filtros = { false, false, true, true, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        //        bool[] ocultos = { true, true, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false };


        //        string[] AnchoColumna = { "180px", "130px", "150px", "150px", "150px", "250px", "150px", "100px", "150px", "150px", "150px", "100px", "100px", "100px", "150px" };
        //        ClickFactura_Entidades.BD.Modelos.TablaGeneralModel tabla = new ClickFactura_Entidades.BD.Modelos.TablaGeneralModel();
        //        tabla.Seleccionar = false;
        //        tabla.NombreCreate = "Crear";
        //        tabla.NombreUpdate = "Actualizar";
        //        tabla.Control = "OrdenyRecepcion";
        //        tabla._Edicion = "onEdit";
        //        tabla._Guardar = "onSave";
        //        tabla.NombreExcel = "excel.xlsx";
        //        tabla.NombreGrid = "GridRecepciones";
        //        tabla.Agrupar = true;
        //        tabla.Numerico = false;
        //        var combo = "<select onchange=\"Combo2()\">";
        //        string opciones = "<option value='0'>Seleccione</option>";
        //        foreach (var item in facturas)
        //        {
        //            if (facturas.Count() == 1)
        //            {
        //                opciones += "<option selected value='" + item.Archivo + "'>" + item.Archivo + "</option>";
        //                break;
        //            }
        //            else
        //            {
        //                opciones += "<option value='" + item.Archivo + "'>" + item.Archivo + "</option>";
        //            }

        //        }
        //        combo += opciones + "</select>";
        //        template[5] = combo;

        //        int x = 1;
        //        foreach (var col in columnas)
        //        {
        //            tabla.Columnas.Add(new ClickFactura_Entidades.BD.Modelos.Columna()
        //            {
        //                Nombre = col.Key,
        //                Tipo = col.Value,
        //                oculto = col.Key.Contains("Id") ? true : ocultos[x - 1],
        //                filtreable = false,
        //                Ancho = AnchoColumna[x - 1],
        //                ClientTemplate = template[x - 1]
        //            });
        //            tabla.NombresModel.Add("R" + x);
        //            x += 1;
        //        }
        //        tabla.CamposAgrupados = new Dictionary<string, Type>();
        //        tabla.CamposAgrupados.Add("R1", tabla.Columnas[0].Tipo);
        //        tabla.CamposAgrupados.Add("R2", tabla.Columnas[1].Tipo);
        //        objFacturasAPasivos = new List<ClickFactura_Facturacion.Clases.paraVerificacionFactura.obj_facturaAPasivo>();
        //        foreach (var item in registros)
        //        {
        //            if (facturas.Count() == 1)
        //            {
        //                item.R18 = facturas[0].Archivo;
        //            }
        //            else
        //            {
        //                item.R18 = "";
        //            }
        //            ClickFactura_Facturacion.Clases.paraVerificacionFactura.obj_facturaAPasivo obje = new ClickFactura_Facturacion.Clases.paraVerificacionFactura.obj_facturaAPasivo();
        //            obje.OrdenCompra = ordenCompra;
        //            obje.Posicion = item.R1.ToString();
        //            obje.NoRecepcion = item.R2.ToString();
        //            obje.NoItem = item.R4.ToString() == "Q" ? item.R15.ToString() : item.R3.ToString();
        //            obje.Posicion_Documento_Material = item.R3.ToString();
        //            obje.FechaFactura = item.R5.ToString();
        //            obje.Piezas = item.R6.ToString();
        //            obje.Importe = item.R7.ToString();
        //            obje.Impuesto = item.R9.ToString();
        //            obje.Año = item.R4.ToString() == "Q" ? item.R15.ToString() : item.R13.ToString();
        //            obje.Documento_Referencia = item.R16.ToString();
        //            obje.ImporteUnitario = Convert.ToDouble(item.R17);
        //            obje.NombreArchivo = item.R18 != null ? item.R18.ToString() : "";
        //            obje.EsdocumentoReferencia = item.R4.ToString() == "Q" ? "S" : "N";
        //            obje.DiferenciaImportes = 0;
        //            #region Actualizacion del viejo al nuevo Portal GRD 5 Dic 2016
        //            if (facturas != null && facturas.Count() > 0)
        //            {
        //                var existeFactura = from f in facturas where f.NombreArchivo.Equals(obje.NombreArchivo) == true select f;
        //                if (existeFactura != null && existeFactura.Count() > 0)
        //                {
        //                    obje.ImporteDocumento = Convert.ToDouble(facturas[0].SubTotal);
        //                    obje.IvaTrasladadoTotal = facturas[0].ImporteTrasladado;
        //                }
        //            }

        //            #endregion Actualizacion del viejo al nuevo Portal GRD 5 Dic 2016
        //            objFacturasAPasivos.Add(obje);
        //            List<object> reg = new List<object>();
        //            reg.Add(ordenCompra);
        //            reg.Add(obje.NoRecepcion);                  //R2
        //            reg.Add(obje.Posicion);                     //R1
        //            reg.Add(obje.NoItem);                       //R3
        //            reg.Add(obje.Posicion_Documento_Material);  //R3
        //            reg.Add(obje.NombreArchivo);                //Factura
        //            reg.Add(obje.Importe);                      //R7
        //            reg.Add("reloj.png");
        //            reg.Add("En espera...");
        //            reg.Add(obje.Impuesto);                     //R9
        //            tabla.Registros.Add(new ClickFactura_Entidades.BD.Modelos.Registros().MapeaDatos(reg));
        //            x += 1;
        //        }

        //        return tabla;
        //    }
        //    catch (Exception ex)
        //    {
        //        mensajes.Add(ex.Message);
        //        return null;
        //    }
        //}

        #endregion Carga información de la Tabla de Recepciones

    }

}