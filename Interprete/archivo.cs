using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBScript.Interprete
{
    public class archivo
    {
        public List<funcion> funciones = new List<funcion>();
        Stack<funcion> pila_funciones = new Stack<funcion>();

        funcion principal;
        public funcion global;
        String ruta= "C:/Users/wirock/Documents/Compi2/default/";
        Double incerteza = 0.5;
        funcion funcion_actual;

        public archivo() { }

        public void ejecutar_global() {
            @const.funcion_global = global;
            ejecutar.ejecutar_gobal(global);
            
        }

        public void ejecutar_principal() {
            ejecutar.ejecutar_flujo(principal,null);
            //Console.WriteLine("LO QUE HAY EN GLOBAL");
            //foreach (variable variable in global.variables.lista)
            //{
            //    Console.WriteLine(variable.tipo + " " + variable.nombre + " " + variable.valor);
            //}
        }




        public void set_ruta_global() {
            @const.RUTA = this.ruta;
            //probar que la ruta exista
            String ruta = @const.RUTA;
            ruta = ruta.Replace("/", "\\\\").Replace("\"", "");
            if (Directory.Exists(ruta))
            {
                Directory.Delete(ruta,true);
            }
        }

        public void set_ruta(String ruta) { this.ruta = ruta; }
        public String get_ruta() { return this.ruta; }

        public void set_incerteza(Double incerteza) { this.incerteza = incerteza; }
        public Double get_incerteza() { return this.incerteza; }

        //OPERACIONES SOBRE FUNCIONES
        public void nueva_funcion(String nombre,funcion padre,String tipo) {
            funcion_actual = new funcion(nombre,padre,tipo);
            pila_funciones.Push(funcion_actual);
        }
        public void pop_funcion() {
            funciones.Add(funcion_actual);
            pila_funciones.Pop();
            funcion_actual = pila_funciones.Peek();
        }
        public funcion extraer_funcion() {
            funcion ret = funcion_actual;
            pila_funciones.Pop();
            funcion_actual = pila_funciones.Peek();
            return ret;
        }
        public void set_principal() {
            principal = funcion_actual;
            pila_funciones.Pop();
            funcion_actual = pila_funciones.Peek();


            //QUITAR ESTO DESPUES
            //@const.dibujar_funcion(principal);
        }
        public void set_global() {
            global = funcion_actual;
            pila_funciones.Pop();
        }
        public funcion get_actual() {
            return funcion_actual;
        }
        //OPERACIONES SOBRE SENTENCIAS
        public void add_sentencia(sentencia s) {
            funcion_actual.add_sentencia(s);
        }

        public void add_parametro(String tipo,String nombre) {
            funcion_actual.add_parametro(tipo,nombre);
        }
        public void agregar_nombre(String s) {
            funcion_actual.agregar_nombre(s);
        }
        //shows
        public void show() {
            Console.WriteLine("----------------------------");
            show_ruta();
            show_incerteza();
            Console.WriteLine("----------------------------");
            show_funciones();
            Console.WriteLine("----------------------------");

        }

        void show_ruta() {
            Console.WriteLine("ruta: " + ruta);
        }

        void show_incerteza() {
            Console.WriteLine("incerteza: " + incerteza);
        }

        public void show_funcion(funcion funcion) {
            Console.WriteLine(funcion.nombre);
            Console.WriteLine("Parametros: ");
            foreach (String parametro in funcion.parametros) {
                Console.WriteLine(parametro);
            }
            Console.WriteLine("Sentencias: ");
            foreach (sentencia s in funcion.sentencias)
            {
                String tipo = s.tipo;
                if (tipo.Equals("declarar")||tipo.Equals("asignacion"))
                {
                    String ids = "";
                    foreach (String id in s.ids)
                    {
                        ids += id + ",";
                    }
                    if (ids.Length > 0)
                        ids = ids.Substring(0, ids.Length - 1);
                    String asigna = "";
                    if (s.expresion.raiz != null)
                        asigna = "= expresion";
                    String cadena = tipo + " " + ids + " " + asigna;
                    Console.WriteLine(cadena);
                }
            }
        }

        public void show_funciones() {
            show_funcion(global);
            Console.WriteLine("----------------------------");
            show_funcion(principal);
            Console.WriteLine("----------------------------");
            foreach (funcion fun in funciones) {
                show_funcion(fun);
                Console.WriteLine("----------------------------");
            }
        }
    }
}
