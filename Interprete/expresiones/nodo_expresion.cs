using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBScript.Interprete.expresiones
{
    public class nodo_expresion
    {
        public String valor,rol,tipo;
        public nodo_expresion izq, der;
        public List<arbol_expresion> parametros = new List<arbol_expresion>();
        public int fila, columna;
        public nodo_expresion(String valor,String rol,String tipo,int fila,int columna) {
            this.valor = valor;
            this.rol = rol;
            this.tipo = tipo;
            this.fila = fila;
            this.columna = columna;
        }
    }
}
