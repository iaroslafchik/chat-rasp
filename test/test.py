import urllib.request
import urllib.parse
import json

def make_requests():
    base_url = "http://localhost:8080/api/schedule/group/{}/save"
    
    for group_id in range(560, 566):  # от 560 до 565 включительно
        # Формируем URL с параметрами
        params = {
            "start": "2026.02.23",
            "finish": "2026.03.01",
            "lng": "1"
        }
        query_string = urllib.parse.urlencode(params)
        url = f"{base_url.format(group_id)}?{query_string}"
        
        try:
            print(f"Отправка запроса для группы {group_id}...")
            
            # Создаем POST-запрос с пустым телом
            data = b''  # пустое тело запроса
            req = urllib.request.Request(url, data=data, method='POST')
            
            # Выполняем запрос
            with urllib.request.urlopen(req) as response:
                status_code = response.getcode()
                response_text = response.read().decode('utf-8')
                
                if status_code == 200:
                    print(f"✓ Группа {group_id}: Успешно (статус {status_code})")
                else:
                    print(f"✗ Группа {group_id}: Ошибка (статус {status_code})")
                    
        except urllib.error.HTTPError as e:
            print(f"✗ Группа {group_id}: HTTP ошибка {e.code}")
            print(f"  Ответ: {e.read().decode('utf-8')[:200]}")
            
        except urllib.error.URLError as e:
            print(f"✗ Группа {group_id}: Ошибка подключения - {e.reason}")
            
        except Exception as e:
            print(f"✗ Группа {group_id}: Неожиданная ошибка - {e}")
    
    print("\n✓ Все запросы выполнены!")

if __name__ == "__main__":
    make_requests()