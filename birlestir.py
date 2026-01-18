import os

# --- AYARLAR ---
extensions = ['.cs'] # .cs Unity için önemli
output_filename = 'tum_kodlar.txt'

# Masaüstü yolu
desktop_path = os.path.join(os.path.expanduser("~"), "Desktop")
output_file_path = os.path.join(desktop_path, output_filename)

def merge_files():
    print(f"İşlem başladı... Hedef: {output_file_path}")
    
    # Denenecek kodlama formatları (Türkçe karakter sorunu için)
    encodings_to_try = ['utf-8', 'cp1254', 'latin-1']

    try:
        with open(output_file_path, 'w', encoding='utf-8') as outfile:
            for root, dirs, files in os.walk("."):
                
                # Çıktı dosyasını okuma
                if output_filename in files and root == desktop_path:
                    files.remove(output_filename)
                
                for file in files:
                    if any(file.endswith(ext) for ext in extensions):
                        file_path = os.path.join(root, file)
                        print(f"İşleniyor: {file_path}")
                        
                        outfile.write(f"\n{'='*25}\n")
                        outfile.write(f" DOSYA YOLU: {file_path}\n")
                        outfile.write(f"{'='*25}\n\n")
                        
                        # Farklı formatları sırayla dene
                        content = None
                        for enc in encodings_to_try:
                            try:
                                with open(file_path, 'r', encoding=enc) as infile:
                                    content = infile.read()
                                break # Başarılıysa döngüden çık
                            except UnicodeDecodeError:
                                continue # Hata verirse diğer formatı dene
                        
                        if content is not None:
                            outfile.write(content)
                            outfile.write("\n")
                        else:
                            outfile.write("!!! HATA: Dosya hiçbir formatla okunamadı.\n")

        print(f"\n✅ BAŞARILI! Dosya şurada: {output_file_path}")

    except Exception as e:
        print(f"\n❌ GENEL HATA: {e}")

if __name__ == "__main__":
    merge_files()