/*************************************************************************
The MIT License (MIT)

Copyright (c) 2014 Yury Tsoy

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Atoms
{
    /// <summary>
    /// TODO: Implement autosorting tree. Can be useful for the dendrograms and also in other algorithms.
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class Tree<T>
	{
		public TreeNode<T> Root { get; set; }

		public Tree()
		{
			Root = new TreeNode<T>() { Name = "Root" };
		}

		public TreeNode<T> FindByName(string name)
		{
			return Root.FindByName(name);
		}

		public TreeNode<T> FindByData(T data)
		{
			return Root.FindByData(data);
		}

        public List<TreeNode<T>> GetLeaves()
        {
            return Root.GetLeaves();
        }

        /// <summary>
        /// Returns all nodes of a tree represented by a list.
        /// The depth traversal is used.
        /// </summary>
        /// <returns></returns>
        public List<TreeNode<T>> GetAllNodes()
        {
            var res = new List<TreeNode<T>>();
            res.Add(Root);
            res.AddRange(Root.GetAllNodes());
            return res;
        }
	}

    /// <summary>
    /// TODO: implement removal, and move of nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class TreeNode<T>
	{
        #region - Properties and attributes. -
        public string Name;
        public T Data;

        TreeNode<T> _parent = null;
        public TreeNode<T> Parent { get { return _parent; } }

        List<TreeNode<T>> _children = new List<TreeNode<T>>();
        public List<TreeNode<T>> Children { get { return _children; } }

        int _depth = 0;
        public int Depth { get { return _depth; } }

        public object Tag; 
        #endregion

        #region - Tree operations. -
        public void AddNode(TreeNode<T> child)
        {
            _children.Add(child);
            child._parent = this;
            child._depth = _depth + 1;
        } 
        #endregion

		public TreeNode<T> FindByName(string name)
		{ 
			if (Name == name) return this;

			foreach (var child in Children)
			{
				var tmp = child.FindByName(name);
				if (tmp != null) return tmp;
			}
			return null;
		}

		public TreeNode<T> FindByData(T data)
		{
			if (Data.Equals (data)) return this;

			foreach (var child in Children)
			{
				var tmp = child.FindByData(data);
				if (tmp != null) return tmp;
			}
			return null;
		}

        public List<TreeNode<T>> GetPath ()
        {
            var res = new List<TreeNode<T>>();
            var tmp = _parent;
            while (tmp != null)
            {
                res.Add(tmp);
                tmp = tmp._parent;
            }
            res.Reverse();  // to make the path go from root to the node.
            return res;
        }

        public List<TreeNode<T>> GetLeaves()
        {
            var res = new List<TreeNode<T>>();
            foreach (var child in _children)
            {
                if (child.Children.Count > 0)
                {
                    var tmp = child.GetLeaves();
                    res.AddRange(tmp);
                }
                else { res.Add(child); }
            }
            return res;
        }

        public List<TreeNode<T>> GetAllNodes()
        {
            var res = new List<TreeNode<T>>();
            foreach (var child in _children)
            {
                res.Add(child);
                if (child.Children.Count > 0)
                {
                    var tmp = child.GetAllNodes();
                    res.AddRange(tmp);
                }
            }
            return res;
        }
	}
}
