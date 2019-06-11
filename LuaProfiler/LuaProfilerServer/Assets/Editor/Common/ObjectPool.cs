﻿/*
               #########                       
              ############                     
              #############                    
             ##  ###########                   
            ###  ###### #####                  
            ### #######   ####                 
           ###  ########## ####                
          ####  ########### ####               
         ####   ###########  #####             
        #####   ### ########   #####           
       #####   ###   ########   ######         
      ######   ###  ###########   ######       
     ######   #### ##############  ######      
    #######  #####################  ######     
    #######  ######################  ######    
   #######  ###### #################  ######   
   #######  ###### ###### #########   ######   
   #######    ##  ######   ######     ######   
   #######        ######    #####     #####    
    ######        #####     #####     ####     
     #####        ####      #####     ###      
      #####       ###        ###      #        
        ###       ###        ###               
         ##       ###        ###               
__________#_______####_______####______________
                我们的未来没有BUG              
* ==============================================================================
* Filename: ObjectPool
* Created:  2018/7/13 14:29:22
* Author:   エル・プサイ・コングリィ
* Purpose:  
* ==============================================================================
*/

#if UNITY_EDITOR || USE_LUA_PROFILER
using System;
using System.Collections.Generic;

namespace MikuLuaProfiler
{
    public class ObjectPool<T> where T : class, new()
    {
        public delegate T CreateFunc();

        public ObjectPool()
        {

        }
        public ObjectPool(int poolSize)
        {
            Init(poolSize);
        }
        public T GetObject()
        {
            lock (this)
            {
                if (m_objStack.Count > 0)
                {
                    T t = m_objStack.Pop();
                    return t;
                }
            }
            return new T();
        }

        public void Init(int poolSize, CreateFunc createFunc = null, Action<T> resetAction = null)
        {
            m_objStack = new Stack<T>();
            for (int i = 0; i < poolSize; i++)
            {
                T item = new T();
                m_objStack.Push(item);
            }
        }

        public void Store(T obj)
        {
            if (obj == null)
                return;
            lock (this)
            {
                m_objStack.Push(obj);
            }
        }

        // 少用，调用这个池的作用就没有了
        public void Clear()
        {
            if (m_objStack != null)
                m_objStack.Clear();
        }

        public int Count
        {
            get
            {
                if (m_objStack == null)
                    return 0;
                return m_objStack.Count;
            }
        }

        public Stack<T>.Enumerator GetIter()
        {
            if (m_objStack == null)
                return new Stack<T>.Enumerator();
            return m_objStack.GetEnumerator();
        }

        private Stack<T> m_objStack = null;
    }

    public class ListObjectPool<T> where T : class
    {
        public delegate T CreateFunc();

        public ListObjectPool(int poolSize, CreateFunc createFunc, Action<T> resetAction = null)
        {
            Init(poolSize, createFunc, resetAction);
        }
        public T GetObject()
        {
            T t;
            if (m_topIndex >= 0)
            {
                t = m_objStack[m_topIndex];
                m_topIndex--;
                if (m_resetAction != null)
                {
                    m_resetAction(t);
                }

            }
            else
            {
                t = m_createFunc();
                m_objStack.Add(t);
            }
            return t;
        }

        public void Init(int poolSize, CreateFunc createFunc = null, Action<T> resetAction = null)
        {
            m_objStack = new List<T>();
            m_resetAction = resetAction;
            m_createFunc = createFunc;
            for (int i = 0; i < poolSize; i++)
            {
                T item = m_createFunc();
                m_objStack.Add(item);
            }
            m_topIndex = poolSize - 1;
        }

        public void Store()
        {
            for (int i = Math.Max(0, m_topIndex); i < m_objStack.Count; i++)
            {
                if (m_resetAction != null)
                {
                    m_resetAction(m_objStack[i]);
                }
            }
            m_topIndex = m_objStack.Count - 1;
        }

        // 少用，调用这个池的作用就没有了
        public void Clear()
        {
            if (m_objStack != null)
                m_objStack.Clear();
        }

        public int Count
        {
            get
            {
                if (m_objStack == null)
                    return 0;
                return m_objStack.Count;
            }
        }

        public List<T>.Enumerator GetIter()
        {
            if (m_objStack == null)
                return new List<T>.Enumerator();
            return m_objStack.GetEnumerator();
        }
        private int m_topIndex = 0;
        private List<T> m_objStack = null;
        private Action<T> m_resetAction = null;
        private CreateFunc m_createFunc = null;
    }
}
#endif