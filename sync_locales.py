import os
import xml.etree.ElementTree as ET
import json

def resx_to_json(resx_path):
    translations = {}
    try:
        tree = ET.parse(resx_path)
        root = tree.getroot()
        for data in root.findall('data'):
            name = data.get('name')
            value_element = data.find('value')
            if name and value_element is not None:
                translations[name] = value_element.text
    except Exception as e:
        print(f"Error parsing {resx_path}: {e}")
    return translations

def main():
    base_dir = os.path.dirname(os.path.abspath(__file__))
    resources_dir = os.path.join(base_dir, 'Resources')
    wwwroot_locales_dir = os.path.join(base_dir, 'wwwroot', 'locales')

    if not os.path.exists(wwwroot_locales_dir):
        os.makedirs(wwwroot_locales_dir)

    languages = ['en', 'hi', 'gu']
    
    for lang in languages:
        resx_filename = f'SharedResource.{lang}.resx' if lang != 'en' else 'SharedResource.en.resx'
        # Note: Sometimes default is just SharedResource.resx, but user has SharedResource.en.resx
        
        resx_path = os.path.join(resources_dir, resx_filename)
        
        if os.path.exists(resx_path):
            print(f"Processing {resx_filename}...")
            data = resx_to_json(resx_path)
            
            json_filename = f'{lang}.json'
            json_path = os.path.join(wwwroot_locales_dir, json_filename)
            
            with open(json_path, 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
            print(f"Created {json_path}")
        else:
            print(f"Warning: {resx_filename} not found.")

if __name__ == "__main__":
    main()
