using SBScript.Interprete.expresiones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SBScript.Interprete
{
    public class sentencia
    {
        public String tipo;
        public List<String> ids;
        public arbol_expresion expresion;
        public String tipo_dato="";
        public List<camino> caminos;
        public sentencia inicial;
        public int fila, columna;

        public sentencia() { }
        public sentencia(String tipo, List<String> ids, arbol_expresion expresion,String tipo_dato,List<camino> caminos,int fila, int columna) {
            this.tipo = tipo;
            this.ids = ids;
            this.expresion = expresion;
            this.tipo_dato = tipo_dato;
            this.caminos = caminos;
            this.fila = fila;
            this.columna = columna;
        }
        public sentencia(String tipo, List<String> ids, arbol_expresion expresion,int fila,int columna)
        {
            this.tipo = tipo;
            this.ids = ids;
            this.expresion = expresion;
            this.fila = fila;
            this.columna = columna;
        }
        public sentencia(String tipo, sentencia inicial, List<camino> caminos,int fila,int columna) {
            this.tipo = tipo;
            this.inicial = inicial;
            this.caminos = caminos;
            this.fila = fila;
            this.columna = columna;
        }
    }
}
