using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBScript.Interprete.expresiones;
using SBScript.Interprete;
using System.IO;
using SBScript.errores;
using System.Diagnostics;

namespace SBScript
{
    public static class @const
    {
        public static int ERROR = 100;
        public static int ACEPTA = 101;
        static int cluster = 0;
        static String nodos = "";
        static int subgraph = 0;
        //static String sentencias = "";
        static StreamWriter fichero;
        static String archivo;
        static Stack<int> pila_nodos = new Stack<int>();
        static bool color = true;
        static Double incerteza_actual = 0.5;
        public static nodo_expresion OK= new nodo_expresion("1", "1", "1", 1, 1);
        public static nodo_expresion VOID = new nodo_expresion("void", "void", "void", 1, 1);
        public static bool global = false;
        public static funcion funcion_global = null;
        public static interprete interprete = null;
        public static String RUTA = "";
        public static int expresion = 0;
        public static bool bandera_incerteza = true;
        public static Stack<Double> incerteza = new Stack<Double>();
        public static String get_nombre_expresion() {
            return "expresion" + expresion++;
        }


        public static void set_incerteza(Double incerteza) {
            if (bandera_incerteza == true)
            {
                @const.incerteza.Push(incerteza);
                incerteza_actual = incerteza;
                bandera_incerteza = false;
            }
        }

        public static void actualizar_incerteza(Double incerteza) {
            incerteza_actual = incerteza;
            @const.incerteza.Push(incerteza);
        }

        public static Double get_incerteza() {
            return incerteza_actual;
        }

        public static void dibujar_expresion(nodo_expresion raiz,String nombre) {
            //cluster = 0;
            nodos = "";
            String cod = "graph \"\" \n { \n label = \"" + nombre + "\"; \n subgraph cluster01{\n label = \"\" \n";
            dibujar_nodo(raiz);
            cod += nodos;
            cod += " } \n } ";

            System.IO.StreamWriter file = new System.IO.StreamWriter(nombre + ".dot");
            file.WriteLine(cod);
            file.Close();


            String comando = "dot -Tpng " + nombre + ".dot > " +@const.RUTA.Replace("\\","\\\\")+"\\EXP_"+ nombre + ".png";
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + comando);
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            string result = proc.StandardOutput.ReadToEnd();
            Console.WriteLine(result);
        }

        static int dibujar_nodo(nodo_expresion raiz) {
            int nodo = get_cluster();
            String parentesis = "";
            if (raiz.rol.Equals("llamar_funcion")) {
                parentesis = "( )";
            }
            String cadena = "nodo" + nodo + "[label=\"" + raiz.valor+parentesis + "\"];\n";
            if (raiz.rol.Equals("llamar_funcion")) {
                foreach (arbol_expresion arbol in raiz.parametros)
                {
                    cadena += "nodo" + nodo + "--" + "{ nodo" + dibujar_nodo(arbol.raiz) + "} ;\n";
                }
            }
            
            if (raiz.izq != null)
                cadena += "nodo"+nodo + "--" + "nodo" + dibujar_nodo(raiz.izq)+";\n";
            if (raiz.der != null)
                cadena += "nodo" + nodo + "--" + "nodo" + dibujar_nodo(raiz.der) + ";\n";
            nodos += cadena;
            return nodo;
        }

        static int get_cluster() {
            return cluster++;
        }

        static int get_subgraph() { return subgraph++; }

        static String get_color_grafo() {
            if (color==true) { color=!color; return "lightgrey"; }
            else { color = !color; return "lightblue3"; }
        }

        static String get_color()
        {
            if (color == true) { return "lightgrey"; }
            else { return "lightblue3"; }
        }

        public static void dibujar_funcion(funcion fun) {
            //CREAR EL ARCHIVO
            crear_archivo(fun.nombre + ".dot");
            pila_nodos = new Stack<int>();


            int inicio = get_cluster();
            //sentencias = "";
            //String cod = "digraph subgraphs { rankdir = UD label = \"" + fun.nombre + "\"  node[shape = record color = lightgrey]";
            write("digraph subgraphs { rankdir = UD label = \"" + fun.nombre + "\"  node[shape = record color = lightgrey]");
            String parametros = "";
            if (fun.parametros.Count > 0) { 
                parametros = "|{";
                foreach (String parametro in fun.parametros) {
                    parametros += parametro.Replace(',', ' ')+ "|";
                }
                parametros = parametros.Substring(0, parametros.Length - 1) + "}";
            }
            //cod += "node"+inicio+"[label=\"{ "+fun.tipo+":"+fun.nombre+parametros+" } \" ]";
            write("node" + inicio + "[label=\"{ " + fun.tipo + ":" + fun.nombre + parametros + " } \" ]");

            //write("node" + inicio + "->node"+(inicio+1)+";");


            pila_nodos.Push(inicio);

            //int actual=-1;
            foreach (sentencia s in fun.sentencias) {
                dibujar_sentencia(s);
            }

            int ultima = pila_nodos.Pop();
            int fin = get_cluster();
            write("node" + fin + "[label=\"{fin} \" ]");
            write("node" + ultima + "->node"+fin);

            //cod += sentencias;

            //cod += "}";
            write("}");

            /*
            System.IO.StreamWriter file = new System.IO.StreamWriter(fun.nombre + ".dot");
            file.WriteLine(cod);
            file.Close();
            */

            String comando = "dot -Tpng " + fun.nombre + ".dot > " + @const.RUTA.Replace("\\", "\\\\") + "\\AST_" + fun.nombre + ".png";
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + comando);
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            string result = proc.StandardOutput.ReadToEnd();
            Console.WriteLine(result);

        }

        static int dibujar_sentencia(sentencia s) {
            String tipo = s.tipo;
            int no_sentencia = get_cluster();
            if (tipo.Equals("declarar") || tipo.Equals("asignacion"))
            {
                String ids = "|{";
                String tipo_dato = "";
                if (s.tipo_dato != null)
                    tipo_dato = "\\n" + s.tipo_dato;
                foreach (String id in s.ids)
                {
                    ids += id + "|";
                }
                if (ids.Length > 0)
                    ids = ids.Substring(0, ids.Length - 1) + "}";
                String asigna = "";
                if (s.expresion.raiz != null)
                    asigna = "|expresion";
                String cadena = "node" + no_sentencia + "[label=\"{" + tipo + tipo_dato + ids + asigna + "}\"]\n";
                //sentencias += cadena;
                write(cadena);
            }
            else if (tipo.Equals("retorno"))
            {
                String expresion = "";
                if (s.expresion.raiz != null)
                    expresion = "|expresion";
                String cadena = "node" + no_sentencia + "[label=\"{retorno " + expresion + "}\"];";
                //sentencias += cadena;
                write(cadena);
            }
            else if (tipo.Equals("continuar"))
            {
                String cadena = "node" + no_sentencia + "[label=\"{continuar}\"];";
                //sentencias += cadena;
                write(cadena);
            }
            else if (tipo.Equals("detener"))
            {
                String cadena = "node" + no_sentencia + "[label=\"{detener}\"];";
                //sentencias += cadena;
                write(cadena);
            }
            else if (tipo.Equals("call_funcion"))
            {
                nodo_expresion llamada = s.expresion.raiz;
                String parametros = "|{";
                foreach (arbol_expresion parametro in llamada.parametros)
                {
                    parametros += "exp" + "|";
                }
                if (parametros.Length > 0)
                    parametros = parametros.Substring(0, parametros.Length - 1) + "}";
                String nombre = llamada.valor+"( )";
                String cadena = "node" + no_sentencia + "[label=\"{llamar funcion|"+nombre+parametros+"}\"]\n";
                //sentencias += cadena;
                write(cadena);
            }
            else if (tipo.Equals("si"))
            {
                int sentencia_anterior = pila_nodos.Pop();
                int punto = get_cluster();
                write("node" + sentencia_anterior + "->node" + no_sentencia + "\n");

                write("subgraph cluster_" + get_subgraph() + "{ label = \"\"\nrankdir=UD\nstyle=filled\ncolor = " + get_color_grafo() + "\nnode[color = white shape = record]\n");
                write("node" + no_sentencia + "[label=\"si\" color=white shape=diamond];");
                write("node" + punto + "[shape=point];");

                //ci
                pila_nodos.Push(no_sentencia);
                funcion si = s.caminos.ElementAt(0).funcion;
                for (int i = 0; i < si.sentencias.Count; i++)
                {
                    sentencia sent = si.sentencias.ElementAt(i);
                    dibujar_sentencia(sent);
                    if (i == 0)
                        write("[label=si];");
                }
                int pre = pila_nodos.Pop();
                write("node" + pre + "->node" + punto + "\n");
                pila_nodos.Push(punto);

                //no
                if (s.caminos.Count > 1)
                {
                    pila_nodos.Push(no_sentencia);
                    funcion no = s.caminos.ElementAt(1).funcion;
                    for (int i = 0; i < no.sentencias.Count; i++)
                    {
                        sentencia sent = no.sentencias.ElementAt(i);
                        dibujar_sentencia(sent);
                        if (i == 0)
                            write("[label=sino];");
                    }
                    int presino = pila_nodos.Pop();
                    write("node" + presino + "->node" + punto + "\n");
                }
                else
                {
                    write("node" + no_sentencia + "->node" + punto + "[label=\"sino\"];\n");
                }
                write("\n}");
                return -1;
            }
            else if (tipo.Equals("selecciona"))
            {
                int sentencia_anterior = pila_nodos.Pop();
                int punto = get_cluster();
                write("node" + sentencia_anterior + "->node" + no_sentencia + "\n");

                write("subgraph cluster_" + get_subgraph() + "{ label = \"\"\nstyle=filled\ncolor = " + get_color_grafo() + "\nnode[color = white shape = record]\n");
                write("node" + no_sentencia + "[label=\"selecciona\" color=white shape=diamond];");
                write("node" + punto + "[shape=point];");

                //insertar caminos
                foreach (camino c in s.caminos)
                {
                    write("subgraph cluster_" + get_subgraph() + "{ label = \"\"\nrankdir=UD\nstyle=filled\ncolor = " + get_color_grafo() + "\nnode[color = white shape = record]\n");
                    pila_nodos.Push(no_sentencia);
                    funcion flujo = c.funcion;
                    for (int i = 0; i < flujo.sentencias.Count; i++)
                    {
                        sentencia sent = flujo.sentencias.ElementAt(i);
                        dibujar_sentencia(sent);
                        if (i == 0)
                        {
                            if (c.condicion.raiz != null)
                            {
                                write("[label=\"" + c.condicion.raiz.der.valor + "\"];");
                            }
                            else
                            {
                                write("[label=\"Defecto\"];");
                            }
                        }

                    }
                    int precamino = pila_nodos.Pop();
                    write("node" + precamino + "->node" + punto + "\n");

                    write("\n}\n");
                }
                pila_nodos.Push(punto);
                write("\n}");
                return -1;
            }
            else if (tipo.Equals("para"))
            {
                int sentencia_anterior = pila_nodos.Pop();
                int punto = get_cluster();
                write("node" + sentencia_anterior + "->node" + no_sentencia + "\n");

                write("subgraph cluster_" + get_subgraph() + "{ label = \"\"\nstyle=filled\ncolor = " + get_color_grafo() + "\nrankdir=UD\n\nnode[color = white shape = record]\n");
                write("node" + no_sentencia + "[label=\"para\" color=white shape=diamond];");
                write("node" + punto + "[shape=point];");

                pila_nodos.Push(no_sentencia);

                dibujar_sentencia(s.inicial);

                no_sentencia = get_cluster();

                sentencia_anterior = pila_nodos.Pop();
                //int punto = get_cluster();
                write("node" + sentencia_anterior + "->node" + no_sentencia + "\n");

                write("node" + no_sentencia + "[label=\"cumple\" color=white shape=diamond];");
                //write("node" + punto + "[shape=point];");

                
                //insertar caminos
                foreach (camino c in s.caminos)
                {
                    pila_nodos.Push(no_sentencia);
                    funcion flujo = c.funcion;
                    for (int i = 0; i < flujo.sentencias.Count; i++)
                    {
                        sentencia sent = flujo.sentencias.ElementAt(i);
                        dibujar_sentencia(sent);
                        if (i == 0)
                        {
                                write("[label=\"Si\"];");
                        }

                    }
                    int precamino = pila_nodos.Pop();
                    write("node" + precamino + "->node" + no_sentencia + "\n");

                }
                pila_nodos.Push(punto);

                write("node" + no_sentencia + "->node" + punto + "[label=\"no\"]\n");

                write("\n}");
                return -1;
            }
            else if (tipo.Equals("hasta"))
            {
                int sentencia_anterior = pila_nodos.Pop();
                int punto = get_cluster();
                write("node" + sentencia_anterior + "->node" + no_sentencia + "\n");

                write("subgraph cluster_" + get_subgraph() + "{ label = \"\"\nstyle=filled\ncolor = " + get_color_grafo() + "\nnode[color = white shape = record]\n");
                write("node" + no_sentencia + "[label=\"hasta\" color=white shape=diamond];");
                write("node" + punto + "[shape=point];");

                //insertar caminos
                foreach (camino c in s.caminos)
                {
                    pila_nodos.Push(no_sentencia);
                    funcion flujo = c.funcion;
                    for (int i = 0; i < flujo.sentencias.Count; i++)
                    {
                        sentencia sent = flujo.sentencias.ElementAt(i);
                        dibujar_sentencia(sent);
                        if (i == 0)
                        {
                             write("[label=\"No cumple\"];");
                        }

                    }
                    int ultimo = pila_nodos.Pop();
                    write("node" + ultimo + "->node" + no_sentencia + "\n");
                }

                write("node" + no_sentencia + "->node" + punto + "[label=\"cumple\"]\n");

                pila_nodos.Push(punto);
                write("\n}");
                return -1;
            }
            else if (tipo.Equals("mientras"))
            {
                int sentencia_anterior = pila_nodos.Pop();
                int punto = get_cluster();
                write("node" + sentencia_anterior + "->node" + no_sentencia + "\n");

                write("subgraph cluster_" + get_subgraph() + "{ label = \"\"\nstyle=filled\ncolor = " + get_color_grafo() + "\nnode[color = white shape = record]\n");
                write("node" + no_sentencia + "[label=\"mientras\" color=white shape=diamond];");
                write("node" + punto + "[shape=point];");

                //insertar caminos
                foreach (camino c in s.caminos)
                {
                    pila_nodos.Push(no_sentencia);
                    funcion flujo = c.funcion;
                    for (int i = 0; i < flujo.sentencias.Count; i++)
                    {
                        sentencia sent = flujo.sentencias.ElementAt(i);
                        dibujar_sentencia(sent);
                        if (i == 0)
                        {
                            write("[label=\"cumple\"];");
                        }

                    }
                    int ultimo = pila_nodos.Pop();
                    write("node" + ultimo + "->node" + no_sentencia + "\n");
                }

                write("node" + no_sentencia + "->node" + punto + "[label=\"no cumple\"]\n");

                pila_nodos.Push(punto);
                write("\n}");
                return -1;
            }
            //ESTO YA NO ES DE LOS IFS
            int ant = pila_nodos.Pop();
            write("node" + ant + "->node" + no_sentencia + "\n");
            pila_nodos.Push(no_sentencia);

            return no_sentencia;
        }

        static void crear_archivo(String nombre) {
            fichero = File.CreateText(nombre);
            fichero.Close();
            fichero = null;
            archivo = nombre;
        }
        static void write(String cadena) {
            fichero = File.AppendText(archivo);
            fichero.WriteLine(cadena);
            fichero.Close();
            fichero = null;
        }

        public static void reporte_errores(){
            String css = ".datagrid table { border-collapse: collapse; text-align: left; width: 100%;  }  .datagrid { 	font: normal 12px/150% Arial, Helvetica, sans-serif; background: #fff; overflow: hidden; border: 1px solid #36752D; -webkit-border-radius: 3px; -moz-border-radius: 3px; border-radius: 3px; }.datagrid table td, .datagrid table th { padding: 3px 10px; }.datagrid table thead th {background:-webkit-gradient( linear, left top, left bottom, color-stop(0.05, #36752D), color-stop(1, #275420) );background:-moz-linear-gradient( center top, #36752D 5%, #275420 100% );filter:progid:DXImageTransform.Microsoft.gradient(startColorstr='#36752D', endColorstr='#275420');background-color:#36752D; color:#FFFFFF; font-size: 15px; font-weight: bold; border-left: 1px solid #36752D; } .datagrid table thead th:first-child { border: none; }.datagrid table tbody td { color: #275420; border-left: 1px solid #C6FFC2;font-size: 12px;font-weight: normal; }.datagrid table tbody .alt td { background: #DFFFDE; color: #275420; }.datagrid table tbody td:first-child { border-left: none; }.datagrid table tbody tr:last-child td { border-bottom: none; }.datagrid table tfoot td div { border-top: 1px solid #36752D;background: #DFFFDE;} .datagrid table tfoot td { padding: 0; font-size: 12px } .datagrid table tfoot td div{ padding: 2px; }.datagrid table tfoot td ul { margin: 0; padding:0; list-style: none; text-align: right; }.datagrid table tfoot  li { display: inline; }.datagrid table tfoot li a { text-decoration: none; display: inline-block;  padding: 2px 8px; margin: 1px;color: #FFFFFF;border: 1px solid #36752D;-webkit-border-radius: 3px; -moz-border-radius: 3px; border-radius: 3px; background:-webkit-gradient( linear, left top, left bottom, color-stop(0.05, #36752D), color-stop(1, #275420) );background:-moz-linear-gradient( center top, #36752D 5%, #275420 100% );filter:progid:DXImageTransform.Microsoft.gradient(startColorstr='#36752D', endColorstr='#275420');background-color:#36752D; }.datagrid table tfoot ul.active, .datagrid table tfoot ul a:hover { text-decoration: none;border-color: #275420; color: #FFFFFF; background: none; background-color:#36752D;}div.dhtmlx_window_active, div.dhx_modal_cover_dv { position: fixed !important; }";
            System.IO.StreamWriter filecss = new System.IO.StreamWriter("datagrid.css");
            filecss.WriteLine(css);
            filecss.Close();


            String codigo = "<div class=\"datagrid\"><table><link rel = \"stylesheet\" href =\"datagrid.css\" ><thead><tr><th>REPORTE DE ERRORES</th></tr></thead><tr>";
            codigo +=DateTime.Now.ToString()+ "</thead><tr><th>Tipo</th><th>Descripcion</th><th>Fila</th><th>Columna</th></tr></thead><tbody>";

            bool bandera = true;
            foreach (errores.error e in errores.errores.lista) {
                if (bandera) {
                    codigo += "<tr><td>"+e.tipo+"</td><td>"+e.descripcion+"</td><td>"+e.fila+"</td><td>"+e.columna+"</td></tr>";
                }
                else {
                    codigo += "<tr class=\"alt\"><td>" + e.tipo + "</td><td>" + e.descripcion + "</td><td>" + e.fila + "</td><td>" + e.columna + "</td></tr>";
                }

                bandera = !bandera;
            }

            codigo += "</tbody></ table ></ div >";


            System.IO.StreamWriter file = new System.IO.StreamWriter("reporte_errores.html");
            file.WriteLine(codigo);
            file.Close();


            Process.Start("reporte_errores.html");

        }
    }
}
