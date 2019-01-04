var html;

function init() {
  iframe = document.getElementsByTagName('iframe')[0];
  iframe.src = "PopCommand.html";
  dowhile();
}
function leerArchivo() {
	var htm = '';
  fetch('source.html')
  .then(response => {
	  return response.text()
  
  
  }
  
  //{
//	  if(html != content)
//	  {
//		  alert(html);
//		  html=content;
//		  iframe.src = 'source.html';
//	  }
//  }
  
  ).then(text => {
	  if(html != text)
	  {
		  html=text;
		  iframe.src = 'source.html';
	  }
	  }
  );
  
}

function dowhile() {
  setTimeout(function () { dowhile(); }, 1500); // refresh every second
    leerArchivo();//iframe.src = 'source.html';
}

