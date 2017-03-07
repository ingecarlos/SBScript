using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBScript.Interprete.expresiones;
using System.IO;

namespace SBScript.Interprete
{
    public static class ejecutar
    {

        public static Stack<funcion> funcion_actual = new Stack<funcion>();

        public static Stack<variables> variables_actuales= new Stack<variables>();

        
        public static nodo_expresion ejecutar_flujo(funcion funcion,List<arbol_expresion> parametros) {
            variables variables = new variables();
            variables_actuales.Push(variables);
            funcion_actual.Push(funcion);

            //DECLARAR PARAMETROS AUN NO HECHA
            int indice = 0;
            foreach (String parametro in funcion.parametros) {
                String tipo = parametro.Split(',')[0].ToLower();
                String id= parametro.Split(',')[1];

                //nodo_expresion valor = parametros.ElementAt(indice).ejecutar_arbol();
                nodo_expresion valor = parametros.ElementAt(indice).ejecutar_arbol();
                if (valor.tipo.Equals("error"))
                {
                    return new nodo_expresion("Error Semantico", "Parametro incorrecto", "error", -1, -1);
                }
                else {
                    variables_actuales.Peek().add_variable(tipo, id, valor.valor);
                }
                indice++;
            }

            foreach (sentencia s in funcion.sentencias) {
                nodo_expresion respuesta= ejecutar_sentencia(s);
                if (respuesta.rol.Equals("retorno"))
                {
                    if (respuesta.tipo.Equals(funcion.tipo.ToLower()))
                    {
                        variables_actuales.Pop();
                        funcion_actual.Pop();
                        return respuesta;
                    }
                    else
                    {
                        errores.errores.add_error("Error Semantico", "Error en retorno de tipo " + respuesta.tipo + " en funcion \"" + funcion.nombre.Split('#')[0] + "\" de tipo " + funcion.tipo, s.fila, s.columna);
                        variables_actuales.Pop();
                        funcion_actual.Pop();
                        return new nodo_expresion("error", "error", "error", s.fila, s.columna);
                    }
                }
                else if (respuesta.rol.Equals("continuar")|| respuesta.rol.Equals("detener")) {
                    errores.errores.add_error("Error Semantico", "Sentencia " + respuesta.rol + " fuera de ciclo, en funcion \"" + funcion.nombre.Split('#')[0] + "\" de tipo " + funcion.tipo, s.fila, s.columna);
                    variables_actuales.Pop();
                    funcion_actual.Pop();
                    return new nodo_expresion("error", "error", "error", s.fila, s.columna);
                }
            }

            //reporte de variables
            //foreach (variable variable in variables_actuales.Peek().lista) {
            //    Console.WriteLine(variable.tipo + " " + variable.nombre + " " + variable.valor);
            //}
            if (!@const.global)
                variables_actuales.Pop();
            else
                funcion.variables = variables_actuales.Pop();
            funcion_actual.Pop();


            if ("void".Equals(funcion.tipo.ToLower()))
            {
                return @const.VOID;
            }
            else
            {
                errores.errores.add_error("Error Semantico", "Error en retorno de tipo " + "void" + " en funcion \"" + funcion.nombre.Split('#')[0] + "\" de tipo " + funcion.tipo, funcion.sentencias.Last().fila, funcion.sentencias.Last().columna);
                return new nodo_expresion("error", "error", "error", funcion.sentencias.Last().fila, funcion.sentencias.Last().columna);
            }

        }

        public static nodo_expresion ejecutar_sentencia_control(funcion funcion)
        {
            variables variables = new variables();
            variables_actuales.Push(variables);
            funcion_actual.Push(funcion);
            foreach (sentencia s in funcion.sentencias)
            {
                nodo_expresion respuesta = ejecutar_sentencia(s);
                if (respuesta.rol.Equals("retorno"))
                {
                    variables_actuales.Pop();
                    funcion_actual.Pop();
                    return respuesta;
                }
                else if (respuesta.rol.Equals("detener"))
                {
                    variables_actuales.Pop();
                    funcion_actual.Pop();
                    return respuesta;
                }
                else if (respuesta.rol.Equals("continuar")) {
                    variables_actuales.Pop();
                    funcion_actual.Pop();
                    return respuesta;
                }
            }
            variables_actuales.Pop();
            funcion_actual.Pop();
            return @const.VOID;
        }



        public static void ejecutar_gobal(funcion funcion) {
            @const.global = true;
            ejecutar_flujo(funcion,null);
            @const.global = false;
        }

        static nodo_expresion ejecutar_sentencia(sentencia sentencia) {
            String tipo = sentencia.tipo;
            if (tipo.Equals("declarar"))
            {
                return declarar(sentencia);
            }
            else if (tipo.Equals("asignacion"))
            {
                return asignacion(sentencia);
            }
            else if (tipo.Equals("call_funcion")) {
                return call_funcion(sentencia);
            }
            else if (tipo.Equals("retorno"))
            {
                return retorno(sentencia);
            }
            else if (tipo.Equals("si"))
            {
                return si(sentencia);
            }
            else if (tipo.Equals("selecciona"))
            {
                return selecciona(sentencia);
            }
            else if (tipo.Equals("detener"))
            {
                return detener(sentencia);
            }
            else if (tipo.Equals("para"))
            {
                return para(sentencia);
            }
            else if (tipo.Equals("continuar"))
            {
                return continuar(sentencia);
            }
            else if (tipo.Equals("mientras"))
            {
                return mientras(sentencia);
            }
            else if (tipo.Equals("hasta"))
            {
                return hasta(sentencia);
            }
            else return null;
        }

        private static nodo_expresion hasta(sentencia sentencia)
        {
            //hacer ciclo
            String condicion = sentencia.caminos.ElementAt(0).condicion.ejecutar_arbol().valor;
            while (condicion.Equals("0"))
            {
                nodo_expresion resultado = ejecutar_sentencia_control(sentencia.caminos.ElementAt(0).funcion);
                if (resultado.rol.Equals("retorno"))
                {
                    return resultado;
                }
                else if (resultado.rol.Equals("continuar"))
                {
                    //continue;
                }
                else if (resultado.rol.Equals("detener"))
                {
                    break;
                }
                condicion = sentencia.caminos.ElementAt(0).condicion.ejecutar_arbol().valor;
            }
            return @const.VOID;
        }

        private static nodo_expresion mientras(sentencia sentencia)
        {
            //hacer ciclo
            String condicion = sentencia.caminos.ElementAt(0).condicion.ejecutar_arbol().valor;
            while (condicion.Equals("1"))
            {
                nodo_expresion resultado = ejecutar_sentencia_control(sentencia.caminos.ElementAt(0).funcion);
                if (resultado.rol.Equals("retorno"))
                {
                    return resultado;
                }
                else if (resultado.rol.Equals("continuar"))
                {
                    //continue;
                }
                else if (resultado.rol.Equals("detener"))
                {
                    break;
                }
                condicion = sentencia.caminos.ElementAt(0).condicion.ejecutar_arbol().valor;
            }
            return @const.VOID;
        }

        private static nodo_expresion continuar(sentencia sentencia)
        {
            nodo_expresion respuesta = new nodo_expresion("", "continuar", "void", sentencia.fila, sentencia.columna);
            return respuesta;
        }

        private static nodo_expresion para(sentencia sentencia) {
            //declarar variable inicial

            ejecutar_sentencia(sentencia.inicial);
            //hacer ciclo
            String condicion = sentencia.caminos.ElementAt(0).condicion.ejecutar_arbol().valor;
            while (condicion.Equals("1")) {
                nodo_expresion resultado = ejecutar_sentencia_control(sentencia.caminos.ElementAt(0).funcion);
                if (resultado.rol.Equals("retorno"))
                {
                    return resultado;
                }
                else if (resultado.rol.Equals("continuar"))
                {
                    ejecutar_sentencia(sentencia.caminos.ElementAt(0).funcion.sentencias.Last());
                    //continue;
                }
                else if (resultado.rol.Equals("detener")) {
                    break;
                }
                condicion = sentencia.caminos.ElementAt(0).condicion.ejecutar_arbol().valor;
            }
            return @const.VOID;
        }

        private static nodo_expresion detener(sentencia sentencia)
        {
            nodo_expresion respuesta = new nodo_expresion("", "detener", "void", sentencia.fila, sentencia.columna);
            return respuesta;
        }

        private static nodo_expresion selecciona(sentencia sentencia)
        {
            Boolean entro = false;
            
            foreach (camino camino in sentencia.caminos) {
                String condicion = "";
                if (camino.condicion.raiz != null)
                    condicion = camino.condicion.ejecutar_arbol().valor;
                else
                    condicion = "1";
                funcion flujo = camino.funcion;
                if (condicion.Equals("1")||entro==true) {
                    entro = true;
                    nodo_expresion respuesta=ejecutar_sentencia_control(flujo);
                    if (respuesta.rol.Equals("detener"))
                    {
                        break;
                    }
                    else if (respuesta.rol.Equals("retorno"))
                    {
                        return respuesta;
                    }
                    else if (respuesta.rol.Equals("continuar")) {
                        return respuesta;
                    }
                }
            }

            return @const.VOID;
        }

        private static nodo_expresion si(sentencia sentencia) {
            nodo_expresion si_cond = sentencia.caminos.ElementAt(0).condicion.ejecutar_arbol();
            funcion si = sentencia.caminos.ElementAt(0).funcion;
            funcion sino=null;
            if (sentencia.caminos.Count > 1) {
                sino = sentencia.caminos.ElementAt(1).funcion;
            }

            if (si_cond.valor.Equals("1"))
            {
                return ejecutar_sentencia_control(si);
            }
            else if (sino != null)
            {
                return ejecutar_sentencia_control(sino);
            }
            else {
                return @const.VOID;
            }
        }

        private static nodo_expresion retorno(sentencia sentencia)
        {
            if (sentencia.expresion.raiz != null)
            {
                nodo_expresion respuesta = sentencia.expresion.ejecutar_arbol();
                respuesta.rol = "retorno";
                return respuesta;
            }
            else {
                nodo_expresion respuesta = new nodo_expresion("", "retorno", "void", sentencia.fila, sentencia.columna);
                return respuesta;
            }
        }

        private static nodo_expresion call_funcion(sentencia sentencia)
        {
            String id = sentencia.expresion.raiz.valor.Split('#')[0];
            if (id.Equals("Mostrar")) {
                return mostrar(sentencia);
            }
            if (id.Equals("DibujarEXP"))
            {
                return dibujarEXP(sentencia);
            }
            if (id.Equals("DibujarAST"))
            {
                return dibujarAST(sentencia);
            }
            return sentencia.expresion.ejecutar_arbol();
        }

        static nodo_expresion declarar(sentencia sentencia) {
            //valor a ingresar
            String valor = "";
            if (sentencia.expresion.raiz != null) {
                nodo_expresion nodo_valor = sentencia.expresion.ejecutar_arbol();
                if (!nodo_valor.tipo.Equals("error"))
                {
                    valor = nodo_valor.valor;
                }
                if (sentencia.tipo_dato.Equals("number")) {
                    if (nodo_valor.tipo.Equals("string")) {
                        errores.errores.add_error("Error Semantico", "casteo indefinido string a number", sentencia.fila, sentencia.columna);
                        valor = "";
                    }
                }else if (sentencia.tipo_dato.Equals("bool"))
                {
                    if (!nodo_valor.tipo.Equals("bool")) {
                        errores.errores.add_error("Error Semantico", "casteo indefinido "+nodo_valor.tipo+" a bool", sentencia.fila, sentencia.columna);
                        valor = "";
                    }
                }
            }
            //lista de ids;
            foreach (String id in sentencia.ids) {
                nodo_expresion respuesta = variables_actuales.Peek().add_variable(sentencia.tipo_dato,id,valor);
                if (!respuesta.valor.Equals("1")) {
                    errores.errores.add_error(respuesta.rol, respuesta.valor, sentencia.fila, sentencia.columna);
                }
            }
            return @const.OK;
        }

        static nodo_expresion asignacion(sentencia sentencia) {
            nodo_expresion valor = sentencia.expresion.ejecutar_arbol();
            String id = sentencia.ids.ElementAt(0);
            String tipo = valor.tipo;
            variable variable = null;

            variable = buscar_variable(id);

            if (variable != null)
            {
                String tipo_id = variable.tipo;
                String tipo_exp = valor.tipo;
                if (tipo.Equals("error"))
                    return @const.VOID;
                if (tipo_id.Equals("number"))
                {
                    if (tipo_exp.Equals("string"))
                    {
                        errores.errores.add_error("Error Semantico", "casteo indefinido string a number", sentencia.fila, sentencia.columna);
                        return @const.VOID;
                    }
                }
                else if (tipo_id.Equals("bool"))
                {
                    if (!tipo_exp.Equals("bool"))
                    {
                        errores.errores.add_error("Error Semantico", "casteo indefinido " + tipo_exp + " a bool", sentencia.fila, sentencia.columna);
                        return @const.VOID;
                    }
                }

                variable.valor = valor.valor;
                return @const.VOID;
            }
            else {
                //REPORTAR ERROR
                errores.errores.add_error("Error Semantico", "Variable \"" + id + "\" no definida", sentencia.fila, sentencia.columna);
                return @const.VOID;
            }
        }

        public static variable buscar_variable(String tipo, String nombre) {
            variable variable=null;
            //buscar local en todos los ambitos
            Stack<variables> vars = new Stack<variables>(variables_actuales.Reverse());
            vars.Reverse();
            var aux = vars.Pop();
            while (aux!=null && variable==null) {
                variable = aux.getVariable(tipo, nombre);
                if (vars.Count > 0)
                    aux = vars.Pop();
                else
                    aux = null;
            }

            if (variable == null) {
                foreach (archivo a in @const.interprete.archivos) {
                    variable = a.global.variables.getVariable(tipo, nombre);
                    if (variable != null)
                        return variable;
                }
                //variable = @const.funcion_global.variables.getVariable(tipo, nombre);
            }

            return variable;
        }

        public static variable buscar_variable(String id) {
            variable variable = null;

            //primero local

            Stack<variables> vars = new Stack<variables>(variables_actuales.Reverse());
            var aux = vars.Pop();
            while (aux != null && variable == null)
            {
                variable = aux.getVariable(id);
                if (vars.Count > 0)
                    aux = vars.Pop();
                else
                    aux = null;
            }


            //prueba otros tipos
            if (variable == null) { 
                variable = buscar_variable("bool", id);
                if (variable == null)
                {
                    variable = buscar_variable("number", id);
                    if (variable == null)
                    {
                        variable = buscar_variable("string", id);
                    }
                }
            }
            return variable;
        }

        public static funcion buscar_funcion(String nombre) {
            /*
            foreach (funcion funcion in @const.interprete.actual.funciones)
            {
                if (nombre.Equals(funcion.nombre))
                    return funcion;
            }
            return null;
            */
            foreach (archivo a in @const.interprete.archivos) { 
                foreach (funcion funcion in a.funciones)
                {
                    if (nombre.Equals(funcion.nombre))
                    {
                        @const.incerteza.Push(a.get_incerteza());
                        return funcion;
                    }
                }
            }
            return null;
        }

        public static nodo_expresion mostrar(sentencia sentencia) {
            List<String> expresiones = new List<String>();
            foreach (arbol_expresion expresion in sentencia.expresion.raiz.parametros) {
                nodo_expresion respuesta = expresion.ejecutar_arbol();
                if (respuesta.valor.Equals("error"))
                {
                    errores.errores.add_error("Error Semantico", "Parametros incorrectos en funcion Mostrar", sentencia.fila, sentencia.columna);
                    return respuesta;
                }
                else {
                    expresiones.Add(respuesta.valor);
                }
            }
            String cadena = expresiones.ElementAt(0);
            cadena = cadena.Replace("\\n", "\r\n");
            cadena = cadena.Replace("\\t", "\t");
        
            expresiones.RemoveAt(0);
            String[] parametros = expresiones.ToArray();

            try
            {
                String imprimir = String.Format(cadena, parametros);
                Program.form1.get_consola().AppendText("\r\n>" + imprimir);
            }
            catch (Exception e) {
                errores.errores.add_error("Error Semantico", "Parametros incorrectos en funcion Mostrar", sentencia.fila, sentencia.columna);
                return new nodo_expresion("error","error","error",sentencia.fila,sentencia.columna);
            }
            return @const.VOID;
        }

        public static nodo_expresion dibujarEXP(sentencia sentencia)
        {
            if (sentencia.expresion.raiz.parametros.Count > 1) {
                errores.errores.add_error("Error Semantico", "Parametros incorrectos en funcion DibujarEXP", sentencia.fila, sentencia.columna);
                return new nodo_expresion("error", "error", "error", sentencia.fila, sentencia.columna);
            }

            arbol_expresion expresion= sentencia.expresion.raiz.parametros.ElementAt(0);
            //probar que la ruta exista
            String ruta = @const.RUTA;
            ruta = ruta.Replace("/", "\\\\").Replace("\"", "");
            if (Directory.Exists(ruta))
            {
                @const.dibujar_expresion(expresion.raiz,@const.get_nombre_expresion());
            }
            else
            {
                Directory.CreateDirectory(ruta);
                @const.dibujar_expresion(expresion.raiz, @const.get_nombre_expresion());
            }
            return @const.VOID;
        }

        public static nodo_expresion dibujarAST(sentencia sentencia)
        {
            if (sentencia.expresion.raiz.parametros.Count > 1)
            {
                errores.errores.add_error("Error Semantico", "Parametros incorrectos en funcion DibujarAST", sentencia.fila, sentencia.columna);
                return new nodo_expresion("error", "error", "error", sentencia.fila, sentencia.columna);
            }

            String nombre = sentencia.expresion.raiz.parametros.ElementAt(0).raiz.valor;

            List<funcion> funciones = get_funciones(nombre);

            if (funciones.Count==0) {
                errores.errores.add_error("Error Semantico", "No existen funciones con nombre "+nombre, sentencia.fila, sentencia.columna);
                return new nodo_expresion("error", "error", "error", sentencia.fila, sentencia.columna);
            }


            String ruta = @const.RUTA;
            ruta = ruta.Replace("/", "\\\\").Replace("\"", "");
            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }

            foreach (funcion funcion in funciones) {
                @const.dibujar_funcion(funcion);
            }
            
                
            return @const.VOID;
        }

        static int get_tamaño(){
            return 5;    
        }

        public static List<funcion> get_funciones(String nombre) {
            List<funcion> funciones = new List<funcion>();
            foreach (archivo a in @const.interprete.archivos) { 
                foreach (funcion funcion in a.funciones) {
                    if (nombre.Equals(funcion.nombre.Split('#')[0])) {
                        funciones.Add(funcion);
                    }
                }
            }
            return funciones;
        }
    }


}
