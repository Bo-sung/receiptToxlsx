//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.10
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


using System;
using System.Runtime.InteropServices;

namespace Noesis
{

public partial class UIPropertyMetadata : PropertyMetadata {
  internal new static UIPropertyMetadata CreateProxy(IntPtr cPtr, bool cMemoryOwn) {
    return new UIPropertyMetadata(cPtr, cMemoryOwn);
  }

  internal UIPropertyMetadata(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) {
  }

  internal static HandleRef getCPtr(UIPropertyMetadata obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  new internal static IntPtr GetStaticType() {
    IntPtr ret = NoesisGUI_PINVOKE.UIPropertyMetadata_GetStaticType();
    return ret;
  }

}

}
