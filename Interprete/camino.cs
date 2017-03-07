using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBScript.Interprete.expresiones;

namespace SBScript.Interprete
{
    public class camino
    {
        public arbol_expresion condicion;
        public funcion funcion;

        public camino(nodo_expresion raiz,funcion funcion) {
            this.condicion = new arbol_expresion();
            this.condicion.raiz = raiz;
            this.funcion = funcion;
        }
    }
}
