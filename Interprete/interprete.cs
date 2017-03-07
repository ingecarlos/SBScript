using System;
using System.Collections.Generic;
using System.Linq;
using SBScript.Interprete.expresiones;
using System.Text;
using System.Threading.Tasks;

namespace SBScript.Interprete
{
    public class interprete
    {
        public archivo actual;
        int indice=-1;
        public List<archivo> archivos = new List<archivo>();
        variables variables = new variables();

        public interprete() { }

        //OPERACIONES SOBRE ARCHIVOS 
        public void archivo_nuevo() {
            actual = new archivo();
            archivos.Add(actual);
            indice++;
            actual.nueva_funcion("global",null,"void");
        }

        public void pop_archivo() {
            actual.set_global();
            indice--;
            if(indice>=0)
                actual = archivos.ElementAt(indice);
        }
        public void set_ruta(String ruta) {
            actual.set_ruta(ruta);
        }
        public void set_incerteza(Double incerteza) {
            actual.set_incerteza(incerteza);
        }

        //OPERACIONES SOBRE FUNCIONES
        public funcion get_funcion() {
            return actual.get_actual();
        }
        public void funcion_nueva(String nombre,funcion padre,String tipo) {
            actual.nueva_funcion(nombre,padre,tipo);
        }
        public void set_principal() {
            actual.set_principal();
        }
        public void pop_funcion() {
            actual.pop_funcion();
        }
        public funcion extraer_funcion() {
            return actual.extraer_funcion();
        }
        public void add_sentencia(String tipo, List<String> ids, arbol_expresion expresion,int fila, int columna) {
            sentencia  s = new sentencia(tipo, ids, expresion,fila,columna);
            actual.add_sentencia(s);
        }
        public void add_sentencia(String tipo, List<String> ids, arbol_expresion expresion,String tipo_dato,int fila, int columna)
        {
            sentencia s = new sentencia(tipo, ids, expresion,tipo_dato,null, fila,columna);
            actual.add_sentencia(s);
        }
        public void add_sentencia(String tipo, arbol_expresion expresion, List<camino> caminos,int fila, int columna)
        {
            sentencia s = new sentencia(tipo, null, expresion,"", caminos, fila,columna);
            actual.add_sentencia(s);
        }
        public void add_sentencia(String tipo, sentencia inicial, List<camino> caminos,int fila, int columna) {
            sentencia s = new sentencia(tipo, inicial, caminos,fila,columna);
            actual.add_sentencia(s);
        }
        public void add_parametro(String tipo, String nombre) {
            actual.add_parametro(tipo, nombre);
        }
        public void agregar_nombre(String s) {
            actual.agregar_nombre(s);
        }

        public void show() {
            foreach (archivo a in archivos) {
                a.show();
            }
        }

        //
        public void ejecutar() {
            actual.set_ruta_global();
            actual.ejecutar_global();
            foreach (archivo a in @const.interprete.archivos) {
                a.ejecutar_global();
            }
            actual.ejecutar_principal();
        }

    }
}
