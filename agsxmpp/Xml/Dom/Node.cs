/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2012 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System.IO;
using System.Xml;
using System.Text;

using agsXMPP.IO;

namespace agsXMPP.Xml.Dom
{
	public enum NodeType
	{
		Document,	// xmlDocument
		Element,	// normal Element
		Text,		// Textnode
		Cdata,		// CDATA Section
		Comment,	// comment
		Declaration	// processing instruction
	}

	/// <summary>
	/// 
	/// </summary>
	public abstract class Node
	{
		internal	Node			Parent;

		private		NodeType		    m_NodeType;
		private		string			    m_Value;
		private		string			    m_Namespace;
		internal	int				    m_Index;
		private     readonly NodeList   m_ChildNodes;

	    protected Node()
		{	
			m_ChildNodes = new NodeList(this);
		}

		public NodeType NodeType
		{
			get { return m_NodeType; }
			set { m_NodeType = value; }
		}

		public virtual string Value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}
		
		public string Namespace
		{
			get { return m_Namespace; }
			set { m_Namespace = value; }			
		}

		public int Index
		{
			get { return m_Index; }
		}

		public NodeList ChildNodes
		{
			get
			{
				return this.m_ChildNodes;
			}
		}

		public void Remove()
		{
			if ( Parent!=null )
			{
				int idx = m_Index;
				Parent.ChildNodes.RemoveAt(idx);
				Parent.ChildNodes.RebuildIndex(idx);
			}			
		}

		public void RemoveAllChildNodes()
		{
			m_ChildNodes.Clear();
		}

		/// <summary>
		/// Appends the given Element as child element
		/// </summary>
		/// <param name="e"></param>
		public virtual void AddChild(Node e)
		{
			m_ChildNodes.Add(e);
		}

	    /// <summary>
		/// returns the Xml, difference to the Xml property is that you can set formatting porperties
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public override string ToString()
		{
			return BuildXml(this, false);
		}

		/// <summary>
		/// returns the Xml, difference to the Xml property is that you can set formatting properties
		/// </summary>
		/// <param name="format"></param>
		/// <param name="indent"></param>
		/// <returns></returns>
		public string ToString(bool pretty)
		{			
			return BuildXml(this, pretty);
		}

		#region << Xml Serializer Functions >>
		
		private string BuildXml(Node e, bool indent)
		{
		    if ( e != null )
			{
				var w = new XmlWriterSettings
				{
				    Encoding = Encoding.UTF8,
				    Indent = indent,
				    IndentChars = "   "
				};
			    using (var sr = new StringWriterWithEncoding(Encoding.UTF8))
			    {
			        using (var xw = XmlWriter.Create(sr, w))
			            WriteTree(this, xw, null);
                    sr.Flush();
			        return sr.ToString();
			    }
			}
		    return "";
		}

	    private void WriteTree(Node e, XmlWriter tw, Node parent) 
		{		
			if (e.NodeType == NodeType.Document)
			{
				//Write the ProcessingInstruction node.
				// <?xml version="1.0" encoding="windows-1252"?> ...
				Document doc = e as Document;
				string pi = null;
				
				if (doc.Version != null)
					pi += "version='" + doc.Version + "'";

				if (doc.Encoding != null)
				{
					if (pi != null)
						pi += " ";
						
					pi += "encoding='" + doc.Encoding + "'";
				}
				
				if (pi != null)
					tw.WriteProcessingInstruction("xml", pi);

				foreach(Node n in e.ChildNodes) 
				{						
					WriteTree(n, tw, e);            
				}				
			}
			else if (e.NodeType == NodeType.Text)
			{
				tw.WriteString(e.Value);
			}
			else if (e.NodeType == NodeType.Comment)
			{
				tw.WriteComment(e.Value);
			}
            else if (e.NodeType == NodeType.Cdata)
            {
                tw.WriteCData(e.Value);
            }
			else if (e.NodeType == NodeType.Element)
			{
				Element el = e as Element;

				if (el.Prefix==null)
					tw.WriteStartElement( el.TagName, el.Namespace);
				else
					tw.WriteStartElement( el.Prefix , el.TagName , el.Namespace);

				// Write Namespace
				if ( (parent == null || parent.Namespace != el.Namespace)
					&& el.Namespace != null
					&& el.Namespace.Length !=0
					)
				{
					if (el.Prefix==null)
						tw.WriteAttributeString("xmlns", null, null, el.Namespace);
					else
						tw.WriteAttributeString("xmlns", el.Prefix, null, el.Namespace);
				}

				foreach (var att in el.Attributes) 
				{
				    if (att.Key.Contains(":"))
				    {
				        var splat = att.Key.Split(':');
                        tw.WriteAttributeString(splat[0], splat[1], null, att.Value);
                    }
                    else
					    tw.WriteAttributeString(att.Key, att.Value);				
				}
			
				//tw.WriteString(el.Value);
		
				if (el.ChildNodes.Count > 0) 
				{
					foreach(Node n in el.ChildNodes) 
					{						
						WriteTree(n, tw, e);            
					}
				    tw.WriteEndElement();
				}    
				else 
				{
					tw.WriteEndElement();
				}
			}
		}
		#endregion

	}
}
