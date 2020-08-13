using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using System.Linq;

namespace WpfMultiStyle
{
    /// <summary>
    /// 实现一个标记扩展，该标记扩展支持根据 XAML 制作的多个静态（XAML 加载时）<see cref="System.Windows.Style"/> 资源引用。
    /// </summary>
    [MarkupExtensionReturnType(typeof(Style))]
    public class MultiStyleExtension : MarkupExtension
    {
        private string[] _internalResourceKeys = new string[0];
        private string _resourceKeys;

        [ConstructorArgument("resourceKeys")]
        public string ResourceKeys
        {
            get
            {
                return _resourceKeys;
            }

            set
            {
                _resourceKeys = value;

                if (_resourceKeys.HasValue())
                {
                    _internalResourceKeys = _resourceKeys.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    _internalResourceKeys = new string[0];
                }
            }
        }

        /// <summary>
        /// 构造方法。
        /// </summary>
        public MultiStyleExtension()
        {
            
        }

        /// <summary>
        /// 构造方法。
        /// </summary>
        /// <param name="resourceKeys">多个<see cref="System.Windows.Style"/>资源字典多个 Key</param>
        public MultiStyleExtension(string resourceKeys)
        {
            ResourceKeys = resourceKeys;
        }

        /// <summary>
        /// 返回一个应在此扩展应用的属性上设置的对象。对于 <see cref="WpfMultiStyle.MultiStyleExtension"/>，这是在资源字典中查找的多个 <see cref="System.Windows.Style"/> 对象，并合并这些对象，其中要查找的对象由 <see cref="System.Windows.StaticResourceExtension.ResourceKey"/> 标识。
        /// </summary>
        /// <param name="serviceProvider">可以为标记扩展提供服务的对象。</param>
        /// <returns>要在计算标记扩展提供的值的属性上设置的对象值。</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Style resultStyle = new Style();

            foreach (string resourceKey in _internalResourceKeys)
            {
                Style currentStyle = TryFindInAmbientResources(serviceProvider, resourceKey)
                    ?? TryFindInStaticResource(serviceProvider, resourceKey);


                if (currentStyle != null)
                    resultStyle.Merge(currentStyle);
            }

            return resultStyle;
        }

        private Style TryFindInAmbientResources(IServiceProvider serviceProvider, string resourceKey)
        {
            var schemaContextProvider = serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider;

            var ambientProvider = serviceProvider.GetService(typeof(IAmbientProvider)) as IAmbientProvider;

            if (schemaContextProvider != null && ambientProvider != null)
            {
                XamlSchemaContext schemaContext = schemaContextProvider.SchemaContext;

                XamlType[] types = new XamlType[1] { schemaContext.GetXamlType(typeof(ResourceDictionary)) };

                IEnumerable<object> allResourceDictionaries = ambientProvider.GetAllAmbientValues(types) as IEnumerable<object>;

                foreach (object resourceDictionaryObject in allResourceDictionaries)
                {
                    ResourceDictionary resourceDictionary = (ResourceDictionary)resourceDictionaryObject;

                    Style currentStyle = resourceDictionary.GetResource<Style>(resourceKey);

                    if (currentStyle != null)
                    {
                        return currentStyle;
                    }
                }
            }

            return null;
        }

        private Style TryFindInStaticResource(IServiceProvider serviceProvider, string resourceKey)
        {
            IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var fe = service.TargetObject as FrameworkElement;

            if (fe != null)
            {
                Style currentStyle = fe.TryFindResource(resourceKey) as Style;

                if (currentStyle != null)
                    return currentStyle;
            }

            return null;
        }
    }
}
