using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBScript.errores;

namespace SBScript.Interprete.expresiones
{
    public class arbol_expresion
    {
        public nodo_expresion raiz;

        public arbol_expresion() { }

        public nodo_expresion ejecutar_arbol() {
            return ejecutar_arbol(raiz);
        }

        public nodo_expresion ejecutar_arbol(nodo_expresion raiz) {
            if (raiz == null)
                return null;
            nodo_expresion valor;

            if (raiz.rol.Equals("operador"))
            {
                nodo_expresion izq = ejecutar_arbol(raiz.izq);
                nodo_expresion der = ejecutar_arbol(raiz.der);
                //ARITMETICOS
                if (raiz.valor.Equals("+"))
                {
                    return suma(izq, der);
                }
                else if (raiz.valor.Equals("-"))
                {
                    return resta(izq, der);
                }
                else if (raiz.valor.Equals("*"))
                {
                    return mult(izq, der);
                }
                else if (raiz.valor.Equals("/"))
                {
                    return div(izq, der);
                }
                else if (raiz.valor.Equals("%"))
                {
                    return mod(izq, der);
                }
                else if (raiz.valor.Equals("^"))
                {
                    return pot(izq, der);
                }
                //RELACIONALES
                else if (raiz.valor.Equals("==") || raiz.valor.Equals("!=") || raiz.valor.Equals("<") || raiz.valor.Equals("<=") || raiz.valor.Equals(">") || raiz.valor.Equals(">=") || raiz.valor.Equals("~"))
                {
                    return relacional(izq, der, raiz.valor);
                }
                //LOGICAS
                else if (raiz.valor.Equals("||"))
                {
                    return or(izq, der);
                }
                else if (raiz.valor.Equals("|&"))
                {
                    return xor(izq, der);
                }
                else if (raiz.valor.Equals("&&"))
                {
                    return and(izq, der);
                }
                else if (raiz.valor.Equals("!"))
                {
                    return not(izq);
                }
            }
            else if (raiz.rol.Equals("terminal"))
            {
                if (!raiz.tipo.Equals("id"))
                {
                    return raiz;
                }
                else
                {
                    return get_id(raiz);
                    //return new nodo_expresion("error", "error", "error", raiz.fila,raiz.columna);
                }
            }
            else if (raiz.rol.Equals("llamar_funcion")) {
                return llamar_funcion(raiz);
            }

            valor= new nodo_expresion("error","error","error", raiz.fila, raiz.columna);
            return valor;
        }

        private nodo_expresion suma(nodo_expresion izq, nodo_expresion der)
        {
            String tipo_izq = izq.tipo, tipo_der = der.tipo;
            if (tipo_izq.Equals("error") || tipo_der.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
            }
            else if (tipo_izq.Equals("string") || tipo_der.Equals("string"))
            {
                //concatenar
                String v1 = izq.valor;
                String v2 = der.valor;
                String result = v1 + v2;
                nodo_expresion nuevo = new nodo_expresion(result, "terminal", "string", raiz.fila, raiz.columna);
                return nuevo;
            }
            else if (tipo_izq.Equals("number") || tipo_der.Equals("number"))
            {
                //suma casual
                Double val1, val2;
                val1 = Double.Parse(izq.valor.Replace(".",","));
                val2 = Double.Parse(der.valor.Replace(".", ","));

                Double valor = val1 + val2;
                String result = valor.ToString();
                nodo_expresion nuevo = new nodo_expresion(result, "terminal", "number", raiz.fila, raiz.columna);
                return nuevo;
            }
            else {
                //or
                Boolean v1=false, v2=false;
                if (izq.valor.Equals("1"))
                    v1 = true;
                if (der.valor.Equals("1"))
                    v2 = true;
                Boolean result = v1 || v2;

                if (result)
                    return new nodo_expresion("1", "terminal", "bool", raiz.fila, raiz.columna);
                else
                    return new nodo_expresion("0", "terminal", "bool", raiz.fila, raiz.columna);
            }

            return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
        }

        private nodo_expresion resta(nodo_expresion izq, nodo_expresion der)
        {
            if (der==null) { return negativo(izq); }

            String tipo_izq = izq.tipo, tipo_der = der.tipo;

            if (tipo_izq.Equals("error") || tipo_der.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
            }
            else if (tipo_izq.Equals("string") || tipo_der.Equals("string"))
            {
                nodo_expresion nuevo = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo_izq + " y " + tipo_der + " con operador -", izq.fila, izq.columna);
                return nuevo;
            }
            else if (tipo_izq.Equals("number") || tipo_der.Equals("number"))
            {
                //suma casual
                Double val1, val2;
                val1 = Double.Parse(izq.valor.Replace(".", ","));
                val2 = Double.Parse(der.valor.Replace(".", ","));

                Double valor = val1 - val2;
                String result = valor.ToString();
                nodo_expresion nuevo = new nodo_expresion(result, "terminal", "number", izq.fila, izq.columna);
                return nuevo;
            }
            else
            {
                nodo_expresion nuevo = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo_izq + " y " + tipo_der + " con operador -", izq.fila, izq.columna);
                return nuevo;
            }

            return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
        }

        private nodo_expresion negativo(nodo_expresion nodo) {
            if (nodo.tipo.Equals("number") || nodo.tipo.Equals("bool"))
            {
                Double valor = Double.Parse(nodo.valor.Replace(".", ","));
                valor = -valor;
                nodo_expresion nuevo = new nodo_expresion(valor.ToString(), "terminal", "number", nodo.fila, nodo.columna);
                return nuevo;
            }
            else {
                errores.errores.add_error("Error Semantico", "imposible operar " + nodo.tipo + " con operador -", nodo.fila, nodo.columna);
                return new nodo_expresion("error", "error", "error", nodo.fila, nodo.columna);
            }
        }

        private nodo_expresion mult(nodo_expresion izq, nodo_expresion der)
        {
            String tipo_izq = izq.tipo, tipo_der = der.tipo;
            if (tipo_izq.Equals("error") || tipo_der.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
            }
            else if (tipo_izq.Equals("string") || tipo_der.Equals("string"))
            {
                nodo_expresion nuevo = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo_izq + " y " + tipo_der + " con operador *", izq.fila, izq.columna);
                return nuevo;
            }
            else if (tipo_izq.Equals("number") || tipo_der.Equals("number"))
            {
                //mult casual
                Double val1, val2;
                val1 = Double.Parse(izq.valor.Replace(".", ","));
                val2 = Double.Parse(der.valor.Replace(".", ","));

                Double valor = val1 * val2;
                String result = valor.ToString();
                nodo_expresion nuevo = new nodo_expresion(result, "terminal", "number", raiz.fila, raiz.columna);
                return nuevo;
            }
            else
            {
                //and
                Boolean v1 = false, v2 = false;
                if (izq.valor.Equals("1"))
                    v1 = true;
                if (der.valor.Equals("1"))
                    v2 = true;
                Boolean result = v1 && v2;

                if (result)
                    return new nodo_expresion("1", "terminal", "bool", raiz.fila, raiz.columna);
                else
                    return new nodo_expresion("0", "terminal", "bool", raiz.fila, raiz.columna);
            }

            return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
        }

        private nodo_expresion div(nodo_expresion izq, nodo_expresion der)
        {
            String tipo_izq = izq.tipo, tipo_der = der.tipo;
            if (tipo_izq.Equals("error") || tipo_der.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
            }
            else if (tipo_izq.Equals("string") || tipo_der.Equals("string"))
            {
                nodo_expresion nuevo = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo_izq + " y " + tipo_der + " con operador /", izq.fila, izq.columna);
                return nuevo;
            }
            else if (tipo_izq.Equals("number") || tipo_der.Equals("number"))
            {
                //mult casual
                Double val1, val2;
                val1 = Double.Parse(izq.valor.Replace(".", ","));
                val2 = Double.Parse(der.valor.Replace(".", ","));
                if (val2 == 0) {
                    nodo_expresion err = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                    errores.errores.add_error("Error Semantico", "division con 0", izq.fila, izq.columna);
                    return err;
                }

                Double valor = val1 / val2;
                String result = valor.ToString();
                nodo_expresion nuevo = new nodo_expresion(result, "terminal", "number", raiz.fila, raiz.columna);
                return nuevo;
            }
            else
            {
                nodo_expresion nuevo = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo_izq + " y " + tipo_der + " con operador /", izq.fila, izq.columna);
                return nuevo;
            }

            return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
        }

        private nodo_expresion mod(nodo_expresion izq, nodo_expresion der)
        {
            String tipo_izq = izq.tipo, tipo_der = der.tipo;
            if (tipo_izq.Equals("error") || tipo_der.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
            }
            else if (tipo_izq.Equals("string") || tipo_der.Equals("string"))
            {
                nodo_expresion nuevo = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo_izq + " y " + tipo_der + " con operador %", izq.fila, izq.columna);
                return nuevo;
            }
            else if (tipo_izq.Equals("number") || tipo_der.Equals("number"))
            {
                //mod casual
                Double val1, val2;
                val1 = Double.Parse(izq.valor.Replace(".", ","));
                val2 = Double.Parse(der.valor.Replace(".", ","));
                if (val2 == 0)
                {
                    nodo_expresion err = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                    errores.errores.add_error("Error Semantico", "division con 0", izq.fila, izq.columna);
                    return err;
                }

                Double valor = val1 % val2;
                String result = valor.ToString();
                nodo_expresion nuevo = new nodo_expresion(result, "terminal", "number", raiz.fila, raiz.columna);
                return nuevo;
            }
            else
            {
                nodo_expresion nuevo = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo_izq + " y " + tipo_der + " con operador %", izq.fila, izq.columna);
                return nuevo;
            }

            return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
        }

        private nodo_expresion pot(nodo_expresion izq, nodo_expresion der)
        {
            String tipo_izq = izq.tipo, tipo_der = der.tipo;
            if (tipo_izq.Equals("error") || tipo_der.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
            }
            else if (tipo_izq.Equals("string") || tipo_der.Equals("string"))
            {
                nodo_expresion nuevo = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo_izq + " y " + tipo_der + " con operador ^", izq.fila, izq.columna);
                return nuevo;
            }
            else if (tipo_izq.Equals("number") || tipo_der.Equals("number"))
            {
                //mod casual
                Double val1, val2;
                val1 = Double.Parse(izq.valor.Replace(".", ","));
                val2 = Double.Parse(der.valor.Replace(".", ","));
                
                Double valor = Math.Pow(val1,val2);
                String result = valor.ToString();
                nodo_expresion nuevo = new nodo_expresion(result, "terminal", "number", raiz.fila, raiz.columna);
                return nuevo;
            }
            else
            {
                nodo_expresion nuevo = new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo_izq + " y " + tipo_der + " con operador %", izq.fila, izq.columna);
                return nuevo;
            }

            return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
        }

        private nodo_expresion relacional(nodo_expresion izq, nodo_expresion der, String op) {
            String v1, v2, tipo1, tipo2;
            v1 = izq.valor;
            v2 = der.valor;
            tipo1 = izq.tipo;
            tipo2 = der.tipo;

            if (tipo1.Equals("error") || tipo2.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", izq.fila, der.fila);
            }
            else if (tipo1.Equals(tipo2))
            {
                if (tipo1.Equals("string")) {
                    if (op.Equals("==")) {
                        int res = comparar_cadenas(v1, v2);
                        if (res == 0)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals("!="))
                    {
                        int res = comparar_cadenas(v1, v2);
                        if (res == 0)
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals("<"))
                    {
                        int res = comparar_cadenas(v1, v2);
                        if (res == 1)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals("<="))
                    {
                        int res = comparar_cadenas(v1, v2);
                        if (res == 0 || res == 1)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals(">"))
                    {
                        int res = comparar_cadenas(v1, v2);
                        if (res == -1)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals(">="))
                    {
                        int res = comparar_cadenas(v1, v2);
                        if (res == -1 || res==0)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals("~"))
                    {
                        int res = comparar_cadenas(v1.Trim(), v2.Trim());
                        if (res == 0)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                }
                else {
                    Double val1 = Double.Parse(v1.Replace(".",","));
                    Double val2 = Double.Parse(v2.Replace(".", ","));
                    if (op.Equals("=="))
                    {
                        bool bandera = val1 == val2;
                        if (bandera==true)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals("!="))
                    {
                        bool bandera = val1 != val2;
                        if (bandera == true)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals("<"))
                    {
                        bool bandera = val1 < val2;
                        if (bandera == true)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals("<="))
                    {
                        bool bandera = val1 <= val2;
                        if (bandera == true)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals(">"))
                    {
                        bool bandera = val1 > val2;
                        if (bandera == true)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals(">="))
                    {
                        bool bandera = val1 >= val2;
                        if (bandera == true)
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        else
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                    }
                    else if (op.Equals("~"))
                    {
                        Double diferencia =Math.Abs(val2 - val1);
                        if (diferencia <= @const.incerteza.Peek())
                        {
                            return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                        }
                        else
                        {
                            return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
                        }
                    }
                }
            }
            else {
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo1 + " y " + tipo2 + " con operador "+op, izq.fila, izq.columna);
                return new nodo_expresion("error", "error", "error", izq.fila, der.fila);
            }
            return new nodo_expresion("error", "error", "error", izq.fila, der.fila);
        }

        private nodo_expresion or(nodo_expresion izq, nodo_expresion der) {
            string valor1, valor2, tipo1, tipo2;
            valor1 = izq.valor;
            valor2 = der.valor;
            tipo1 = izq.tipo;
            tipo2 = der.tipo;
            if (tipo1.Equals("error") || tipo2.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
            }
            else if (tipo1.Equals("bool") && tipo2.Equals("bool"))
            {
                bool v1 = false;
                bool v2 = false;
                if (valor1.Equals("1"))
                    v1 = true;
                if (valor2.Equals("1"))
                    v2 = true;
                bool result = v1 || v2;
                if (result == true)
                    return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                else
                    return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
            }
            else {
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo1 + " y " + tipo2 + " con operador ||" ,izq.fila, izq.columna);
                return new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
            }
        }

        private nodo_expresion xor(nodo_expresion izq, nodo_expresion der)
        {
            string valor1, valor2, tipo1, tipo2;
            valor1 = izq.valor;
            valor2 = der.valor;
            tipo1 = izq.tipo;
            tipo2 = der.tipo;
            if (tipo1.Equals("error") || tipo2.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
            }
            else if (tipo1.Equals("bool") && tipo2.Equals("bool"))
            {
                bool v1 = false;
                bool v2 = false;
                if (valor1.Equals("1"))
                    v1 = true;
                if (valor2.Equals("1"))
                    v2 = true;
                bool result = v1 ^ v2;
                if (result == true)
                    return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                else
                    return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
            }
            else
            {
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo1 + " y " + tipo2 + " con operador |&", izq.fila, izq.columna);
                return new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
            }
        }

        private nodo_expresion and(nodo_expresion izq, nodo_expresion der)
        {
            string valor1, valor2, tipo1, tipo2;
            valor1 = izq.valor;
            valor2 = der.valor;
            tipo1 = izq.tipo;
            tipo2 = der.tipo;
            if (tipo1.Equals("error") || tipo2.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
            }
            else if (tipo1.Equals("bool") && tipo2.Equals("bool"))
            {
                bool v1 = false;
                bool v2 = false;
                if (valor1.Equals("1"))
                    v1 = true;
                if (valor2.Equals("1"))
                    v2 = true;
                bool result = v1 && v2;
                if (result == true)
                    return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                else
                    return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
            }
            else
            {
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo1 + " y " + tipo2 + " con operador &&", izq.fila, izq.columna);
                return new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
            }
        }

        private nodo_expresion not(nodo_expresion izq)
        {
            string valor1, tipo1;
            valor1 = izq.valor;
            tipo1 = izq.tipo;
            if (tipo1.Equals("error"))
            {
                return new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
            }
            else if (tipo1.Equals("bool"))
            {
                bool v1 = false;
                if (valor1.Equals("1"))
                    v1 = true;
                bool result = !v1;
                if (result == true)
                    return new nodo_expresion("1", "terminal", "bool", izq.fila, izq.columna);
                else
                    return new nodo_expresion("0", "terminal", "bool", izq.fila, izq.columna);
            }
            else
            {
                errores.errores.add_error("Error Semantico", "imposible operar " + tipo1 + " con operador !", izq.fila, izq.columna);
                return new nodo_expresion("error", "error", "error", izq.fila, izq.columna);
            }
        }

        private nodo_expresion get_id(nodo_expresion raiz) {
            variable variable = ejecutar.buscar_variable(raiz.valor);
            if (variable != null)
            {
                if (variable.valor.Equals("")) {
                    errores.errores.add_error("Error Semantico", "Variable \"" + raiz.valor + "\" no inicializada", raiz.fila, raiz.columna);
                    return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
                }
                else {
                    return new nodo_expresion(variable.valor, "terminal", variable.tipo, raiz.fila, raiz.columna);
                }
            }
            else {
                errores.errores.add_error("Error Semantico", "Variable \"" + raiz.valor + "\" no definida", raiz.fila, raiz.columna);
                return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
            }
        }

        private nodo_expresion llamar_funcion(nodo_expresion raiz)
        {

            //encontrar funcion
            //generar el nombre
            String nombre_funcion = raiz.valor;

            foreach (arbol_expresion parametro in raiz.parametros)
            {
                nodo_expresion result = parametro.ejecutar_arbol();
                nombre_funcion += "#" + result.tipo;
            }

            //ejecutarla
            funcion funcion = ejecutar.buscar_funcion(nombre_funcion);

            if (funcion != null)
            {
                nodo_expresion respuesta= ejecutar.ejecutar_flujo(funcion, raiz.parametros);
                respuesta.rol = "terminal";
                //AQUI SE VUELVE A SACAR LA INCERTEZA
                @const.incerteza.Pop();
                //---------------------------------
                return respuesta;
            }
            else
            {
                String parametros;
                if (nombre_funcion.Split('#').Length > 1)
                    parametros = "con parametros ";
                else
                    parametros = "";
                for (int i = 1; i < nombre_funcion.Split('#').Length; i++)
                {
                    parametros += nombre_funcion.Split('#')[i];
                }
                errores.errores.add_error("Error Semantico", "la funcion " + nombre_funcion.Split('#')[0] + " " + parametros + " no fue declarada", raiz.fila, raiz.columna);
                return new nodo_expresion("error", "error", "error", raiz.fila, raiz.columna);
            }
        }

        private int comparar_cadenas(String v1, String v2) {
            Char[] str1 = v1.ToCharArray();
            Char[] str2 = v2.ToCharArray();
            int tam1 = str1.Length;
            int tam2 = str2.Length;
            int tam;
            if (tam1 < tam2)
            {
                tam = tam1;
            }
            else
            {
                tam = tam2;
            }
            int mayor = 0;
            for (int i = 0; i < tam; i++)
            {
                int c1 = (int)str1[i];
                int c2 = (int)str2[i];
                if (c1 > c2)
                {
                    mayor = -1;
                    return mayor;
                }
                else if (c1 < c2)
                {
                    mayor = 1;
                    return mayor;
                }
            }

            if (tam1 < tam2)
            {
                mayor = 1;
                return mayor;
            }
            else if (tam1 > tam2)
            {
                mayor = -1;
                return mayor;
            }
            else {
                mayor = 0;
                return mayor;
            }

        }


    }
}
